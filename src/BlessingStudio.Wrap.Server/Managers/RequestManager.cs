using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.Wrap.Protocol.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Server.Managers
{
    public class RequestManager : IEnumerable<RequestInfo>
    {
        private List<RequestInfo> requestInfos = new List<RequestInfo>();
        public IReadOnlyCollection<RequestInfo> RequestInfos { 
            get
            {
                return requestInfos.AsReadOnly();
            } 
        }
        public void AddRequest(RequestInfo requestInfo)
        {
            Remove(requestInfo.Requester, requestInfo.Receiver);
            requestInfos.Add(requestInfo);
            requestInfo.Receiver.Connection.Send("main", new ConnectRequestPacket() { UserToken = requestInfo.Requester.UserToken});
        }
        public void RemoveRequest(RequestInfo requestInfo)
        {
            requestInfos.Remove(requestInfo);
        }
        public void Clear()
        {
            requestInfos.Clear();
        }
        public void RemoveAll(UserInfo userInfo)
        {
            requestInfos.RemoveAll(x => x.Receiver == userInfo || x.Requester == userInfo);
        }
        public void Remove(UserInfo requester, UserInfo receiver)
        {
            requestInfos.RemoveAll(x => x.Requester == requester && x.Receiver == receiver);
        }
        public RequestInfo? Find(UserInfo requester, UserInfo receiver)
        {
            return requestInfos.FirstOrDefault((x) => x.Requester == requester && x.Receiver == receiver);
        }

        public IEnumerator<RequestInfo> GetEnumerator()
        {
            return RequestInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)RequestInfos).GetEnumerator();
        }
    }
}
