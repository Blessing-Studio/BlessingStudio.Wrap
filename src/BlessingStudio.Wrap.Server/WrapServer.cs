using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Interfaces;
using BlessingStudio.Wrap.Protocol;
using BlessingStudio.Wrap.Protocol.Packet;
using BlessingStudio.Wrap.Server.Managers;
using BlessingStudio.Wrap.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Server
{
    public class WrapServer
    {
        public TcpListener Listener;
        public UserManager Users { get; private set; } = new();
        public RequestManager Requests { get; private set; } = new();
        public Dictionary<IConnection, bool> Logined { get; private set; } = new();
        public WrapServer(int port = ConstValue.ServerPort)
        {
            Listener = new(new IPAddress(new byte[4]), port);
        }
        public void Start()
        {
            Listener.Start();
            while (true)
            {
                try
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    Connection connection = new(client.GetStream());
                    Channel mainChannel = connection.CreateChannel("main");
                    Logined[connection] = false;
                    // Add handlers
                    mainChannel.AddHandler((e) =>
                    {
                        if(e.Object is LoginPacket loginPacket)
                        {
                            if (!Logined[e.Connection])
                            {
                                Logined[e.Connection] = true;
                                bool customTokenAvaliable = loginPacket.UseCustomToken;
                                foreach(UserInfo userInfo in Users)
                                {
                                    if(userInfo.UserToken == loginPacket.UserToken)
                                    {
                                        customTokenAvaliable = false;
                                    }
                                }
                                LoginSuccessfulPacket loginSuccessfulPacket = new();
                                if (!customTokenAvaliable)
                                {
                                    loginSuccessfulPacket.UserToken = new RandomString() { GenNumber = true}.Next(32);
                                }
                                else
                                {
                                    loginSuccessfulPacket.UserToken = loginPacket.UserToken;
                                }
                                e.Channel.Send(loginSuccessfulPacket);
                                Users.AddNewUser(connection, loginSuccessfulPacket.UserToken);
                                Console.WriteLine("new user " + loginSuccessfulPacket.UserToken);
                            }
                            else
                            {
                                e.Channel.Send(new DisconnectPacket()
                                {
                                    Reason = "You logined again."
                                });
                                e.Connection.Dispose();
                                Logined.Remove(e.Connection);
                            }
                        }
                    });
                    mainChannel.AddHandler((e) =>
                    {
                        if (!Logined[e.Connection]) return;
                        UserInfo user = Users.Find(e.Connection)!;
                        if(e.Object is ConnectRequestPacket requestPacket)
                        {
                            UserInfo? userInfo = Users.Find(requestPacket.UserToken);
                            if(userInfo != null)
                            {
                                UserInfo sender = Users.Find(e.Connection)!;
                                Requests.AddRequest(new(sender, userInfo));
                            }
                        }
                        else if(e.Object is ConnectAcceptPacket acceptPacket)
                        {
                            UserInfo? receiver = Users.Find(acceptPacket.UserToken);
                            if (receiver != null)
                            {
                                RequestInfo? requestInfo = Requests.Find(receiver, user);
                                if (requestInfo != null)
                                {
                                    receiver.Connection.Send("main", new ConnectAcceptPacket()
                                    {
                                        UserToken = user.UserToken,
                                        IPAddress = acceptPacket.IPAddress,
                                        port = acceptPacket.port
                                    });
                                    Requests.RemoveRequest(requestInfo);
                                }
                            }
                        }
                        else if(e.Object is IPInfoPacket infoPacket)
                        {
                            UserInfo? userInfo = Users.Find(infoPacket.UserToken);
                            if (userInfo != null)
                            {
                                userInfo.Connection.Send("main", new IPInfoPacket()
                                {
                                    IPAddress = infoPacket.IPAddress,
                                    port = infoPacket.port,
                                    UserToken = user.UserToken
                                });
                            }
                        }
                    });
                    connection.Serializers[typeof(IPacket)] = new PacketSerializer();
                    connection.Start();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
