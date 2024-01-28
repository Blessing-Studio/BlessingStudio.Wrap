using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Client;

public sealed class RequestInfo
{
    public string Requester { get; set; }
    public RequestInfo(string requester)
    {
        Requester = requester;
    }
    public override int GetHashCode()
    {
        return Requester.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return obj is RequestInfo request && request.Requester == Requester;
    }
    public static bool operator ==(RequestInfo? left, RequestInfo? right)
    {
        return Equals(left, right);
    }
    public static bool operator !=(RequestInfo? left, RequestInfo? right)
    {
        return !Equals(left, right);
    }
}
