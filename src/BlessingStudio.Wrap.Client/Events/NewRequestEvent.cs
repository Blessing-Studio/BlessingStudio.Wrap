using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public sealed class NewRequestEvent : IEvent
{
    public string Requester { get { return RequestInfo.Requester; } set { RequestInfo.Requester = value; } }
    public RequestInfo RequestInfo { get; set; }
    public NewRequestEvent(RequestInfo requester)
    {
        RequestInfo = requester;
    }
}
