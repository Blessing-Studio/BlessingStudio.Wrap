using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.Wrap.Protocol.Packet;
using BlessingStudio.Wrap.Utils;
using System;
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
        public Dictionary<string, long> KeepAliveData = new Dictionary<string, long>();
        public Thread KeepAliveThread;
        public CancellationTokenSource KeepAliveThreadCancellationTokenSource = new();
        public ushort Nextport = 42000;
        public IPEndPoint Server = new(new IPAddress(new byte[] { 127, 0, 0, 1 }), 25565);
        public List<string> IgnoredConnectionId = new();
        public object PeerSendingLock = new();
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
            //new Thread(() =>
            //{
            //    try
            //    {
            //        while (true)
            //        {
            //            lock (PeerSendingLock)
            //                connection.GetChannel("main").Send(new KeepAlivePacket());
            //        }
            //    }
            //    catch { }
            //}).Start();
            connection.AddHandler((ChannelCreatedEvent e) =>
            {
                if (e.Channel.ChannelName.Contains("connection_") && !IgnoredConnectionId.Contains(e.Channel.ChannelName.Replace("connection_", string.Empty)))
                {
                    new Thread(() =>
                    {
                        try
                        {
                            TcpClient client = new TcpClient();
                            Channel channel = e.Channel;
                            try
                            {
                                client.Connect(Server);
                            }
                            catch
                            {
                                client.Close();
                                lock (PeerSendingLock)
                                    connection.DestroyChannel(channel.ChannelName);
                                return;
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
                                    return;
                                }
                            });
                            connection.AddHandler((ChannelDeletedEvent e2) =>
                            {
                                if (e2.Channel == e.Channel.ChannelName)
                                {
                                    client.Close();
                                }
                            });
                            new Thread(() =>
                            {
                                try
                                {
                                    while (true)
                                    {
                                        byte[] buffer = new byte[4096];
                                        client.Client.Blocking = true;
                                        int c = client.Client.Receive(buffer);
                                        if (c == 0)
                                        {
                                            continue;
                                        }
                                        byte[] bytes = new byte[c];
                                        Array.Copy(buffer, bytes, c);
                                        lock (PeerSendingLock)
                                            channel.Send(bytes);
                                    }
                                }
                                catch
                                {
                                    client.Close();
                                    lock (PeerSendingLock)
                                        connection.DestroyChannel(channel.ChannelName);
                                    return;
                                }
                            }).Start();

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
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
                try
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
                                return;
                            }
                        });
                        Thread.Sleep(500);
                        new Thread(() =>
                        {
                            try
                            {
                                while (true)
                                {
                                    byte[] buffer = new byte[4096];
                                    client.Client.Blocking = true;
                                    int c = client.Client.Receive(buffer);
                                    if (c == 0)
                                    {
                                        continue;
                                    }
                                    byte[] bytes = new byte[c];
                                    Array.Copy(buffer, bytes, c);
                                    lock (PeerSendingLock)
                                        channel.Send(bytes);
                                }
                            }
                            catch
                            {
                                lock (PeerSendingLock)
                                    connection.DestroyChannel(channel.ChannelName);
                                return;
                            }
                        }).Start();
                    }
                    listener.Stop();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
            }).Start();
        }
        public void RemovePeer(string token)
        {
            UserInfo info = UserManager.Find(token)!;
            info.Connection.Dispose();
        }
    }
}
