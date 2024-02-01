using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public class ReconnectPeerEvent : IEvent
{
    public string UserToken { get; set; }
    public ReconnectPeerEvent(string userToken)
    {
        UserToken = userToken;
    }
}
