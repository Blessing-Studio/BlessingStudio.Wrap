using BlessingStudio.WonderNetwork;
using BlessingStudio.Wrap.Client;
using BlessingStudio.Wrap.Client.Events;
using BlessingStudio.Wrap.Managers;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Interfaces;

public interface IWrapClient
{
    TcpClient Client { get; }
    string DisconnectReason { get; }
    bool IsConnected { get; }
    bool IsDisposed { get; }
    IPEndPoint LocalIP { get; }
    Channel? MainChannel { get; }
    PeerManager PeerManager { get; }
    IPEndPoint? RemoteIP { get; }
    List<RequestInfo> Requests { get; }
    Connection? ServerConnection { get; }
    IUPnPService? UPnPService { get; }
    string UserToken { get; }

    event WonderNetwork.Events.EventHandler<ConnectPeerFailedEvent>? ConnectFailed;
    event WonderNetwork.Events.EventHandler<ConnectPeerSuccessfullyEvent>? ConnectPeerSuccessfully;
    event WonderNetwork.Events.EventHandler<ExpectedDisconnectEvent>? ExpectedDisconnect;
    event WonderNetwork.Events.EventHandler<LoginedSuccessfullyEvent>? LoginedSuccessfully;
    event WonderNetwork.Events.EventHandler<NewRequestEvent>? NewRequest;
    event WonderNetwork.Events.EventHandler<ReconnectPeerEvent>? ReconnectPeer;
    event WonderNetwork.Events.EventHandler<RequestInvalidatedEvent>? RequestInvalidated;
    event WonderNetwork.Events.EventHandler<UnexpectedDisconnectEvent>? UnexpectedDisconnect;

    void AcceptRequest(RequestInfo request);
    void Close();
    void Connect(IPAddress address, int port = 38297);
    void Dispose();
    void MakeRequest(string userToken);
    void Start(string userToken = "_");
}