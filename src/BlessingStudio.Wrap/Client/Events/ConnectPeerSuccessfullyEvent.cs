using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.WonderNetwork.Interfaces;

namespace BlessingStudio.Wrap.Client.Events;

public class ConnectPeerSuccessfullyEvent : IEvent
{
    public string UserToken {  get; set; }
    public IConnection Connection { get; set; }
    public ushort Port { get; set; }
    public ConnectPeerSuccessfullyEvent(string userToken, IConnection connection, ushort port)
    {
        UserToken = userToken;
        Connection = connection;
        Port = port;
    }
}
