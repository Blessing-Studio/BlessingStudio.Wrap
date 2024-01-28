﻿using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.Wrap.Client.Events;
using BlessingStudio.Wrap.Client.Managers;
using BlessingStudio.Wrap.Protocol;
using BlessingStudio.Wrap.Protocol.Packet;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Client;

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
    public IPEndPoint LocalIP { get; private set; }
    public bool IsDisposed { get; private set; } = false;
    public event WonderNetwork.Events.EventHandler<NewRequestEvent>? NewRequest;
    public event WonderNetwork.Events.EventHandler<RequestInvalidatedEvent>? RequestInvalidated;
    public event WonderNetwork.Events.EventHandler<ConnectPeerFailedEvent>? ConnectFailed;
    public event WonderNetwork.Events.EventHandler<ConnectPeerSuccessfullyEvent>? ConnectPeerSuccessfully;
    public event WonderNetwork.Events.EventHandler<ExpectedDisconnectEvent>? ExpectedDisconnect;
    public event WonderNetwork.Events.EventHandler<UnexpectedDisconnectEvent>? UnexpectedDisconnect;
    public event WonderNetwork.Events.EventHandler<LoginedSuccessfullyEvent>? LoginedSuccessfully;
    public List<RequestInfo> Requests { get; private set; } = new();
    public WrapClient()
    {
        LocalIP = new(new IPAddress(new byte[] { 0, 0, 0, 0 }), Random.Shared.Next(2000, 60000));
        Client = new();
        Client.Client.ExclusiveAddressUse = false;
        Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        Client.Client.Bind(LocalIP);
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
        if (!IsDisposed)
        {
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
                RemoteIP = new(new IPAddress(loginSuccessfulPacket.IPAddress), loginSuccessfulPacket.port);
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
                    IPInfoPacket infoPacket = ServerConnection.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(15))!;
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
            IPInfoPacket infoPacket = ServerConnection!.WaitFor<IPInfoPacket>("main", TimeSpan.FromSeconds(15))!;
            IPEndPoint peerIP = new(new IPAddress(infoPacket.IPAddress), infoPacket.port);
            TryConnect(peerIP, infoPacket.UserToken);
        }
    }
    private void TryConnect(IPEndPoint peerIP, string token, bool listen = false)
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
            listener.Server.Blocking = false;
            listener.Start();
            task2 = Task.Run(() =>
            {
                while (!successed)
                {
                    Thread.Sleep(10);
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
            ConnectFailed?.Invoke(new(token));
            return;
        }
        NetworkStream networkStream = connectionToPeer.GetStream();
        Connection connection = new(new SafeNetworkStream(networkStream));
        connection.Serializers[typeof(IPacket)] = new PacketSerializer();
        Channel channel = connection.CreateChannel("main");
        PeerManager.AddPeer(token, connection, (IPEndPoint)client.Client.RemoteEndPoint!);

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
        ConnectPeerSuccessfully?.Invoke(new(token, connection));
    }

    private void CheckDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
