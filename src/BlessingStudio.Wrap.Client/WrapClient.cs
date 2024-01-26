using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.WonderNetwork.Utils;
using BlessingStudio.Wrap.Protocol;
using BlessingStudio.Wrap.Protocol.Packet;
using BlessingStudio.Wrap.Utils;
using System.Diagnostics.CodeAnalysis;
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
        public IPEndPoint? RemoteIP { get; private set; }
        public bool IsDisposed { get; private set; } = false;
        public WrapClient()
        {
            Client = new();
        }
        public void Connect(IPAddress address, int port = ConstValue.ServerPort)
        {
            CheckDisposed();
            Close();
            Client.Connect(address, port);
            NetworkStream networkStream = Client.GetStream();
            networkStream.Socket.NoDelay = true;
            ServerConnection = new(new SafeNetworkStream(networkStream));
            ServerConnection.Serializers[typeof(IPacket)] = new PacketSerializer();
        }
        public void Close()
        {
            if (!IsDisposed)
            {
                if (IsConnected)
                {
                    Client.Close();
                    Client = new();
                    Client.ExclusiveAddressUse = false;
                    Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    ServerConnection!.Dispose();
                    MainChannel = null;
                }
                PeerManager.Dispose();
                IsDisposed = true;
            }
        }
        public void Dispose()
        {
            Close();
        }
        public void Start(string userToken = "_")
        {
            CheckDisposed();
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
                    IPEndPoint iPEndPoint = new(new IPAddress(loginSuccessfulPacket.IPAddress), loginSuccessfulPacket.port);
                    RemoteIP = iPEndPoint;
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
                        MainChannel.Send(new ConnectAcceptPacket()
                        {
                            UserToken = connectRequestPacket.UserToken,
                        });
                        IPInfoPacket infoPacket = ServerConnection.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(60))!;
                        IPEndPoint peerIP = new IPEndPoint(new IPAddress(infoPacket.IPAddress), infoPacket.port);
                        TryConnect(peerIP, infoPacket.UserToken);
                    }
                    if (e.Object is ConnectAcceptPacket connectAcceptPacket)
                    {
                        //Begin connect
                        IPInfoPacket infoPacket = ServerConnection.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(60))!;
                        IPEndPoint peerIP = new IPEndPoint(new IPAddress(infoPacket.IPAddress), infoPacket.port);
                        TryConnect(peerIP, connectAcceptPacket.UserToken);
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
                MainChannel.Send(new KeepAlivePacket());
            }
        }
        public void MakeRequest(string userToken)
        {
            CheckDisposed();
            if (IsConnected)
            {
                MainChannel!.Send(new ConnectRequestPacket()
                {
                    UserToken = userToken
                });
            }
        }
        private void TryConnect(IPEndPoint peerIP, string token)
        {
            CheckDisposed();
            TcpClient client = new TcpClient();
            client.ExclusiveAddressUse = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(Client.Client.LocalEndPoint!);
            bool successed = false;
            TcpClient? connectionToPeer = null;
            Task task1 = Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
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
            task1.GetAwaiter().GetResult();
            if (connectionToPeer == null)
            {
                Console.WriteLine("Failed!");
            }
            else
            {
                Console.WriteLine($"Connected! {token}");
                connectionToPeer.Client.Blocking = true;
                NetworkStream networkStream = connectionToPeer.GetStream();
                networkStream.Socket.NoDelay = true;
                Connection connection = new(new SafeNetworkStream(networkStream));
                connection.Serializers[typeof(IPacket)] = new PacketSerializer();
                Channel channel = connection.CreateChannel("main");
                PeerManager.AddPeer(token, connection, (IPEndPoint)client.Client.RemoteEndPoint!);
                connection.AddHandler((ReceivedObjectEvent e) =>
                {
                    if(e.Object is DisconnectPacket packet)
                    {
                        Console.WriteLine(packet.Reason);
                    }
                });
                connection.Start();
            }
        }
        private void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
