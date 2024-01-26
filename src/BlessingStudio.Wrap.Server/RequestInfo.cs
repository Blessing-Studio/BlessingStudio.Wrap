using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Server
{
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
}
