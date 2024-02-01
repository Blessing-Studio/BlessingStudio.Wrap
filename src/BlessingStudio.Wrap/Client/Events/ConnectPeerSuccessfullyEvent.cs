using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.WonderNetwork.Interfaces;

namespace BlessingStudio.Wrap.Client.Events;

public class ConnectPeerSuccessfullyEvent : IEvent
{
    public string UserToken {  get; set; }
    public IConnection Connection { get; set; }
    public ConnectPeerSuccessfullyEvent(string userToken, IConnection connection)
    {
        UserToken = userToken;
        Connection = connection;
    }
}
