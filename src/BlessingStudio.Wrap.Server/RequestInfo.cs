namespace BlessingStudio.Wrap;

public class RequestInfo
{
    public UserInfo Requester { get; set; }
    public UserInfo Receiver { get; set; }
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;
    public RequestInfo(UserInfo requester, UserInfo receiver)
    {
        Requester = requester;
        Receiver = receiver;
    }
}
