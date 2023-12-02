using BlessingStudio.WonderNetwork;
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
                if (e.Object is ConnectRequestPacket connectRequestPacket)
                {
                    TcpClient client = new TcpClient();
                    IPEndPoint iPEndPoint = IPEndPoint.Parse("0.0.0.0");
                    iPEndPoint.Port = new Random().Next(20000, 60000);
                    IPEndPoint remoteIpPoint = StunUtils.GetRemoteIP(iPEndPoint);
                    client.Client.Bind(iPEndPoint);
                    MainChannel.Send(new ConnectAcceptPacket()
                    {
                        UserToken = connectRequestPacket.UserToken,
                        IPAddress = remoteIpPoint.Address.GetAddressBytes(),
                        port = remoteIpPoint.Port,
                    });
                    IPInfoPacket infoPacket = ServerConnection.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(60))!;
                    IPEndPoint peerIP = new IPEndPoint(new IPAddress(infoPacket.IPAddress), infoPacket.port);
                    Task.Run(() =>
                    {
                        bool successed = false;
                        for(int i = 0; i < 10; i++)
                        {
                            try
                            {
                                client.Connect(peerIP);
                            }
                            catch (Exception e)
                            {

                            }
                            finally
                            {
                                successed = true;
                            }
                            if(successed)
                            {
                                break;
                            }
                        }
                        
                    });
                }
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
            if (IsConnected)
            {
                MainChannel!.Send(new ConnectRequestPacket()
                {
                    UserToken = userToken
                });
            }
        }
    }
}
