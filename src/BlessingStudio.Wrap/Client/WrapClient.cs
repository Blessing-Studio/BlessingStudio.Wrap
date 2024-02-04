using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.Wrap.Client.Events;
using BlessingStudio.Wrap.Interfaces;
using BlessingStudio.Wrap.Managers;
using BlessingStudio.Wrap.Protocol;
using BlessingStudio.Wrap.Protocol.Packet;
using BlessingStudio.Wrap.Utils;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Client;

public class WrapClient : IDisposable, IWrapClient
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
    public IPeerManager PeerManager { get; private set; } = new PeerManager();
    public IPEndPoint? RemoteIP { get; private set; }
    public IPEndPoint LocalIP { get; private set; }
    public IUPnPService? UPnPService { get; private set; }
    public bool IsDisposed { get; private set; } = false;
    public event WonderNetwork.Events.EventHandler<NewRequestEvent>? NewRequest;
    public event WonderNetwork.Events.EventHandler<RequestInvalidatedEvent>? RequestInvalidated;
    public event WonderNetwork.Events.EventHandler<ConnectPeerFailedEvent>? ConnectFailed;
    public event WonderNetwork.Events.EventHandler<ConnectPeerSuccessfullyEvent>? ConnectPeerSuccessfully;
    public event WonderNetwork.Events.EventHandler<ExpectedDisconnectEvent>? ExpectedDisconnect;
    public event WonderNetwork.Events.EventHandler<UnexpectedDisconnectEvent>? UnexpectedDisconnect;
    public event WonderNetwork.Events.EventHandler<LoginedSuccessfullyEvent>? LoginedSuccessfully;
    public event WonderNetwork.Events.EventHandler<ReconnectPeerEvent>? ReconnectPeer;
    public List<RequestInfo> Requests { get; private set; } = new();
    public WrapClient()
    {
        LocalIP = new(new IPAddress(new byte[] { 0, 0, 0, 0 }), RandomUtils.GetRandomPort());
        Client = new();
        Client.Client.ExclusiveAddressUse = false;
        Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        Client.Client.Bind(LocalIP);
    }
    public WrapClient(IUPnPService uPnPService) : this()
    {
        UPnPService = uPnPService;
    }
    public void Connect(IPAddress address, int port = ConstValue.ServerPort)
    {
        CheckDisposed();
        Client.Connect(address, port);
        NetworkStream networkStream = Client.GetStream();
        ServerConnection = new(new SafeNetworkStream(networkStream));
        ServerConnection.Serializers[typeof(IPacket)] = new PacketSerializer();
    }
    public void Close()
    {
        Dispose();
    }
    public void Dispose()
    {
        if (!IsDisposed)
        {
            if (RemoteIP != null && UPnPService != null)
            {
                UPnPService.DeletePortMapping(RemoteIP.Port, IUPnPService.SocketProtocol.TCP);
            }
            if (IsConnected)
            {
                Client.Close();
                ServerConnection!.Dispose();
                MainChannel = null;
            }
            PeerManager.Dispose();
            IsDisposed = true;
            Requests.Clear();
            GC.SuppressFinalize(this);
        }
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
                RemoteIP = new(new IPAddress(loginSuccessfulPacket.IPAddress), loginSuccessfulPacket.port);
                UPnPService?.AddPortMapping(RemoteIP.Port, IUPnPService.SocketProtocol.TCP, ((IPEndPoint)Client.Client.LocalEndPoint!).Port, "WrapClient");
                LoginedSuccessfully?.Invoke(new(loginSuccessfulPacket.UserToken));
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
                    if (NewRequest != null)
                    {
                        RequestInfo requestInfo = new(connectRequestPacket.UserToken);
                        Requests.Add(requestInfo);
                        NewRequest(new(requestInfo));
                    }
                }
                else if (e.Object is ConnectAcceptPacket connectAcceptPacket)
                {
                    //Begin connect
                    IPInfoPacket infoPacket = ServerConnection.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(5))!;
                    IPEndPoint peerIP = new(new IPAddress(infoPacket.IPAddress), infoPacket.port);
                    TryConnect(peerIP, connectAcceptPacket.UserToken, true);
                }
                else if (e.Object is ConnectRequestInvalidatedPacket connectRequestInvalidatedPacket)
                {
                    Requests.RemoveAll(x => x.Requester == connectRequestInvalidatedPacket.UserToken);
                    RequestInvalidated?.Invoke(new(connectRequestInvalidatedPacket.UserToken));
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
            if (!ServerConnection.IsDisposed)
            {
                MainChannel.Send(new KeepAlivePacket());
            }
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
    public void AcceptRequest(RequestInfo request)
    {
        CheckDisposed();
        if (!Requests.Contains(request))
        {
            throw new ArgumentException("The request does not exist", nameof(request));
        }
        if (IsConnected)
        {
            //Begin connect
            MainChannel!.Send(new ConnectAcceptPacket()
            {
                UserToken = request.Requester,
            });
            Requests.Remove(request);
            IPInfoPacket infoPacket = ServerConnection!.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(5))!;
            IPEndPoint peerIP = new(new IPAddress(infoPacket.IPAddress), infoPacket.port);
            TryConnect(peerIP, infoPacket.UserToken);
        }
    }
    private void TryConnect(IPEndPoint peerIP, string token, bool listen = false, bool isReconnect = false)
    {
        CheckDisposed();
        TcpClient client = new()
        {
            ExclusiveAddressUse = false
        };
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.Client.Bind(Client.Client.LocalEndPoint!);
        bool successed = false;
        TcpClient? connectionToPeer = null;
        Connection? connection = null;
        Task task1 = Task.Run(() =>
        {
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    client.Connect(peerIP);
                    successed = true;
                    connectionToPeer = client;
                }
                catch { }
                if (successed)
                {
                    break;
                }
            }
        });
        TcpListener? listener = null;
        Task? task2 = null;
        if (listen)
        {
            listener = new(LocalIP)
            {
                ExclusiveAddressUse = false
            };
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Start();
            task2 = Task.Run(() =>
            {
                while (!successed)
                {
                    try
                    {
                        TcpClient peer = listener!.AcceptTcpClient();
                        successed = true;
                        if (connectionToPeer == null)
                            connectionToPeer = peer;
                        else
                            peer.Close();
                    }
                    catch { }
                }

            });
        }
        task1.GetAwaiter().GetResult();
        listener?.Stop();
        if (connectionToPeer == null)
        {
            client.Close();
            if (!isReconnect)
            {
                ReconnectPeer?.Invoke(new(token));
                TryConnect(peerIP, token, !listen, true);
            }
            else
            {
                ConnectFailed?.Invoke(new(token));
            }
            return;
        }
        connection = CreateConnectionToPeer(token, connectionToPeer);
        ConnectPeerSuccessfully?.Invoke(new(token, connection, PeerManager.LastPort));
    }

    private Connection CreateConnectionToPeer(string token, TcpClient connectionToPeer)
    {
        NetworkStream networkStream = connectionToPeer.GetStream();
        Connection connection = new(new SafeNetworkStream(networkStream));
        connection.Serializers[typeof(IPacket)] = new PacketSerializer();
        Channel channel = connection.CreateChannel("main");
        PeerManager.AddPeer(token, connection, (IPEndPoint)connectionToPeer.Client.RemoteEndPoint!);

        bool IsExpectedDisconnect = false;
        channel.AddHandler((ReceivedObjectEvent e) =>
        {
            if (e.Object is DisconnectPacket packet)
            {
                IsExpectedDisconnect = true;
                ExpectedDisconnect?.Invoke(new(token, packet.Reason));
                connection.Dispose();
            }
        });
        connection.AddHandler((DisposedEvent _) =>
        {
            if (!IsExpectedDisconnect)
            {
                UnexpectedDisconnect?.Invoke(new(token));
            }
        });

        connection.Start();
        return connection;
    }

    private void CheckDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
