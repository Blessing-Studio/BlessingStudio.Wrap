using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public sealed class RequestInvalidatedEvent : IEvent
{
    public string Requester { get; set; }
    public RequestInvalidatedEvent(string requester)
    {
        Requester = requester;
    }
}
