using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public class UnexpectedDisconnectEvent : IEvent
{
    public string UserToken { get; set; }
    public UnexpectedDisconnectEvent(string userToken)
    {
        UserToken = userToken;
    }
}
