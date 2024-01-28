using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public class ExpectedDisconnectEvent : IEvent
{
    public string UserToken { get; set; }
    public string Reason { get; set; }
    public ExpectedDisconnectEvent(string userToken, string reason)
    {
        UserToken = userToken;
        Reason = reason;
    }
}
