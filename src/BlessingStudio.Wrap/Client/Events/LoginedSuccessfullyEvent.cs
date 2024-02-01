using BlessingStudio.WonderNetwork.Events;

namespace BlessingStudio.Wrap.Client.Events;

public class LoginedSuccessfullyEvent : IEvent
{
    public string UserToken { get; set; }
    public LoginedSuccessfullyEvent(string userToken)
    {
        UserToken = userToken;
    }
}
