using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.Wrap.Protocol.Packet;
using BlessingStudio.Wrap.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Channel = BlessingStudio.WonderNetwork.Channel;

namespace BlessingStudio.Wrap.Client
{
    public class PeerManager
    {
        public UserManager UserManager { get; set; } = new();
        public Dictionary<string, long> KeepAliveData { get; } = new Dictionary<string, long>();
        public Thread KeepAliveThread { get; }
        public CancellationTokenSource KeepAliveThreadCancellationTokenSource { get; } = new();
        public ushort Nextport { get; set; } = 42000;
        public IPEndPoint Server = new(new IPAddress(new byte[] { 127, 0, 0, 1 }), 25565);
        public List<string> IgnoredConnectionId { get; } = new();
        public ArrayPool<byte> Buffer { get; } = ArrayPool<byte>.Create();
        public object PeerSendingLock { get; } = new();
        public PeerManager()
        {
            KeepAliveThread = new((e) =>
            {
                CancellationToken cancellationToken = (CancellationToken)e!;
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    Thread.Sleep(500);
                    lock (KeepAliveData)
                    {
                        foreach (var pair in KeepAliveData)
                        {
                            if (DateTimeOffset.Now.ToUnixTimeSeconds() - pair.Value > 30)
                            {
                                UserInfo info = UserManager.Find(pair.Key)!;
                                //info.Connection.Send("main", new DisconnectPacket() { Reason = "You didn't send KeepAlivePacket in 30s"});
                                //info.Connection.Dispose();
                            }
                        }
                    }
                }
            });
            KeepAliveThread.Start(KeepAliveThreadCancellationTokenSource.Token);
        }
        public void AddPeer(string token, Connection connection, IPEndPoint ip)
        {
            UserManager.AddNewUser(connection, token, ip);
            new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        lock (PeerSendingLock)
                            connection.GetChannel("main").Send(new KeepAlivePacket());
                    }
                }
                catch { }
            }).Start();
            connection.AddHandler((ChannelCreatedEvent e) =>
            {
                Channel channel = e.Channel;
                if (channel.ChannelName.Contains("connection_") && !IgnoredConnectionId.Contains(channel.ChannelName.Replace("connection_", string.Empty)))
                {
                    new Thread(() =>
                    {
                        TcpClient client = new TcpClient();
                        try
                        {
                            client.Connect(Server);
                            channel.Send(new ConnectSuccessfullyPacket());
                        }
                        catch
                        {
                            client.Close();
                            lock (PeerSendingLock)
                                connection.DestroyChannel(channel.ChannelName);
                        }
                        channel.AddHandler((ReceivedBytesEvent e) =>
                        {
                            try
                            {
                                lock (PeerSendingLock)
                                    client.Client.Send(e.Data);
                            }
                            catch
                            {
                                client.Close();
                                lock (PeerSendingLock)
                                    connection.DestroyChannel(channel.ChannelName);
                            }
                        });
                        connection.AddHandler((ChannelDeletedEvent e2) =>
                        {
                            if (e2.Channel == channel.ChannelName)
                            {
                                client.Close();
                                if(IgnoredConnectionId.Contains(e2.Channel.Replace("connection_", string.Empty)))
                                {
                                    IgnoredConnectionId.Remove(e2.Channel.Replace("connection_", string.Empty));
                                }
                            }
                        });
                        CreateClientReceiver(client, channel).Start();
                    }
                    ).Start();
                }
            });
            connection.AddHandler((DisposedEvent e) =>
            {
                lock (KeepAliveData)
                {
                    KeepAliveData.Remove(token);
                }
            });
            connection.AddHandler((ReceivedObjectEvent e) =>
            {
                if (e.Object is KeepAlivePacket packet)
                {
                    lock (KeepAliveData)
                    {
                        KeepAliveData[token] = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }
                    Debug.WriteLine(token + " Keepalived");
                }
            });
            lock (KeepAliveData)
            {
                KeepAliveData[token] = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            new Thread(() =>
            {
                TcpListener listener = new(new IPAddress(new byte[] { 0, 0, 0, 0 }), Nextport);
                listener.ExclusiveAddressUse = false;
                listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Nextport++;
                listener.Start();
                while (true)
                {
                    if (UserManager.Find(token) == null)
                    {
                        break;
                    }
                    TcpClient client = listener.AcceptTcpClient();
                    string id = new RandomString().Next(8);
                    IgnoredConnectionId.Add(id);
                    Channel channel = connection.CreateChannel($"connection_{id}");
                    channel.AddHandler((ReceivedBytesEvent e) =>
                    {
                        try
                        {
                            lock (PeerSendingLock)
                                client.Client.Send(e.Data);
                        }
                        catch
                        {
                            client.Close();
                            lock (PeerSendingLock)
                                connection.DestroyChannel(channel.ChannelName);
                        }
                    });
                    ConnectSuccessfullyPacket? packet = channel.WaitFor<ConnectSuccessfullyPacket>(TimeSpan.FromSeconds(15));
                    if (packet != null)
                    {
                        CreateClientReceiver(client, channel).Start();
                    }
                    else
                    {
                        client.Close();
                        lock (PeerSendingLock)
                            channel.Connection.DestroyChannel(channel.ChannelName);
                    }
                }
                listener.Stop();
            }).Start();
        }
        public void RemovePeer(string token)
        {
            UserInfo info = UserManager.Find(token)!;
            info.Connection.Dispose();
        }
        private Thread CreateClientReceiver(TcpClient client, Channel channel)
        {
            return new Thread(() =>
            {
                byte[] buffer = Buffer.Rent(4096);
                try
                {
                    while (true)
                    {
                        int c = client.Client.Receive(buffer);
                        if (c == 0)
                        {
                            Thread.Sleep(5);
                            continue;
                        }
                        byte[] bytes = Buffer.Rent(c);
                        Array.Copy(buffer, bytes, c);
                        Buffer.Return(bytes);
                        lock (PeerSendingLock)
                            channel.Send(bytes);
                    }
                }
                catch
                {
                    Buffer.Return(buffer);
                    client.Close();
                    lock (PeerSendingLock)
                        channel.Connection.DestroyChannel(channel.ChannelName);
                }
            });
        }
    }
}
