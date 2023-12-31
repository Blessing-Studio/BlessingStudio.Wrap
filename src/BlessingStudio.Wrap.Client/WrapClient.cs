using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.WonderNetwork.Utils;
using BlessingStudio.Wrap.Protocol;
using BlessingStudio.Wrap.Protocol.Packet;
using BlessingStudio.Wrap.Utils;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Client
{
    public class WrapClient : IDisposable
    {
        public bool IsConnected
        {
            get
            {
                return Client.Connected;
            }
        }
        public Connection? ServerConnection { get; private set; }
        public TcpClient Client { get; private set; }
        public Channel? MainChannel { get; private set; }
        public string UserToken { get; private set; } = string.Empty;
        public string DisconnectReason { get; private set; } = string.Empty;
        public PeerManager PeerManager { get; private set; } = new PeerManager();
        public WrapClient()
        {
            Client = new();
        }
        public void Connect(IPAddress address, int port = ConstValue.ServerPort)
        {
            Close();
            Client.Connect(address, port);
            ServerConnection = new(Client.GetStream());
            ServerConnection.Serializers[typeof(IPacket)] = new PacketSerializer();
        }
        public void Close()
        {
            if (IsConnected)
            {
                Client.Close();
                Client = new();
                ServerConnection!.Dispose();
                MainChannel = null;
            }
        }
        public void Dispose()
        {
            Close();
        }
        public void Start(string userToken = "_")
        {
            if (ServerConnection == null)
            {
                throw new InvalidOperationException();
            }
            MainChannel = ServerConnection.CreateChannel("main");
            // Add handlers

            MainChannel.AddHandler((e) =>
            {
                if (e.Object is LoginSuccessfulPacket loginSuccessfulPacket)
                {
                    UserToken = loginSuccessfulPacket.UserToken;
                    Console.WriteLine(UserToken);
                }
                else if (e.Object is LoginFailedPacket loginFailedPacket)
                {
                    Dispose();
                    DisconnectReason = loginFailedPacket.Reason;
                }
            });

            MainChannel.AddHandler((e) =>
            {
                Task.Run(() =>
                {
                    if (e.Object is ConnectRequestPacket connectRequestPacket)
                    {
                        //Begin connect
                        IPEndPoint iPEndPoint = IPEndPoint.Parse("0.0.0.0");
                        iPEndPoint.Port = new Random().Next(20000, 60000);
                        IPEndPoint remoteIpPoint = StunUtils.GetRemoteIP(iPEndPoint);
                        MainChannel.Send(new ConnectAcceptPacket()
                        {
                            UserToken = connectRequestPacket.UserToken,
                            IPAddress = remoteIpPoint.Address.GetAddressBytes(),
                            port = remoteIpPoint.Port,
                        });
                        Console.WriteLine(remoteIpPoint
                            .ToString());
                        IPInfoPacket infoPacket = ServerConnection.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(60))!;
                        IPEndPoint peerIP = new IPEndPoint(new IPAddress(infoPacket.IPAddress), infoPacket.port);
                        TryConnect(iPEndPoint, peerIP, infoPacket.UserToken);
                    }
                    if (e.Object is ConnectAcceptPacket connectAcceptPacket)
                    {
                        //Begin connect
                        IPEndPoint iPEndPoint = IPEndPoint.Parse("0.0.0.0");
                        iPEndPoint.Port = new Random().Next(20000, 60000);
                        IPEndPoint remoteIpPoint = StunUtils.GetRemoteIP(iPEndPoint);
                        Thread.Sleep(2000);
                        MainChannel.Send(new IPInfoPacket()
                        {
                            UserToken = connectAcceptPacket.UserToken,
                            IPAddress = remoteIpPoint.Address.GetAddressBytes(),
                            port = remoteIpPoint.Port,
                        });
                        Console.WriteLine(remoteIpPoint
                            .ToString());
                        IPEndPoint peerIP = new IPEndPoint(new IPAddress(connectAcceptPacket.IPAddress), connectAcceptPacket.port);
                        TryConnect(iPEndPoint, peerIP, connectAcceptPacket.UserToken);
                    }
                });
            });

            ServerConnection.Start();
            MainChannel.Send(new LoginPacket()
            {
                UseCustomToken = !(userToken == "_"),
                UserToken = userToken
            });
            while (true)
            {
                Thread.Sleep(5000);
                //MainChannel.Send(new KeepAlivePacket());
            }
        }
        public void MakeRequest(string userToken)
        {
            if (IsConnected)
            {
                MainChannel!.Send(new ConnectRequestPacket()
                {
                    UserToken = userToken
                });
            }
        }
        private void TryConnect(IPEndPoint local, IPEndPoint peerIP, string token)
        {
            TcpListener listener = new TcpListener(local);
            listener.ExclusiveAddressUse = false;
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Start();
            TcpClient client = new TcpClient();
            client.ExclusiveAddressUse = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(local);
            bool successed = false;
            TcpClient? connectionToPeer = null;
            Task task1 = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    try
                    {
                        client.Connect(peerIP);
                        successed = true;
                        connectionToPeer = client;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    if (successed)
                    {
                        break;
                    }
                }
            });
            Task task2 = Task.Run(() =>
            {
                listener.Server.Blocking = false;
                while (!successed)
                {
                    try
                    {
                        TcpClient tcpClient = listener.AcceptTcpClient();
                        successed = true;
                        connectionToPeer = tcpClient;
                    }
                    catch { Thread.Sleep(20); }
                }
                listener.Stop();
            });
            task1.GetAwaiter().GetResult();
            if (connectionToPeer == null)
            {
                Console.WriteLine("Failed!");
            }
            else
            {
                Console.WriteLine($"Connected! {token}");
                connectionToPeer.Client.Blocking = true;
                Connection connection = new(connectionToPeer.GetStream());
                connection.Serializers[typeof(IPacket)] = new PacketSerializer();
                Channel channel = connection.CreateChannel("main");
                PeerManager.AddPeer(token, connection);
                new Thread(() =>
                {
                    while(true)
                    {
                        channel.Send(new KeepAlivePacket());
                        Thread.Sleep(2000);
                    }
                }).Start();
                connection.AddHandler((ReceivedObjectEvent e) =>
                {
                    if(e.Object is DisconnectPacket packet)
                    {
                        Console.WriteLine(packet.Reason);
                    }
                });
            }
        }
    }
}
