using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public sealed class ConnectPeerFailedEvent : IEvent
{
    public string UserToken { get; set; }
    public ConnectPeerFailedEvent(string userToken)
    {
        UserToken = userToken;
    }
}
