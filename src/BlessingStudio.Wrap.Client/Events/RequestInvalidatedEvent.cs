using BlessingStudio.WonderNetwork.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Client.Events;

public sealed class RequestInvalidatedEvent : IEvent
{
    public string Requester { get; set; }
    public RequestInvalidatedEvent(string requester)
    {
        Requester = requester;
    }
}
