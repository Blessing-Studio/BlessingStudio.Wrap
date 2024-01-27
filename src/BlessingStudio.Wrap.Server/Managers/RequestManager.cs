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
        public RequestManager()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    lock (requestInfos)
                    {
                        List<RequestInfo> toRemove = new();
                        foreach (RequestInfo info in requestInfos)
                        {
                            if((DateTimeOffset.Now - info.DateTime).TotalSeconds > 60)
                            {
                                toRemove.Add(info);
                            }
                        }
                        foreach (RequestInfo info in toRemove)
                        {
                            RemoveRequest(info);
                        }
                    }
                }
            }).Start();
        }
        public void AddRequest(RequestInfo requestInfo)
        {
            lock (requestInfos)
            {
                Remove(requestInfo.Requester, requestInfo.Receiver);
                requestInfos.Add(requestInfo);
                requestInfo.Receiver.Connection.Send("main", new ConnectRequestPacket() { UserToken = requestInfo.Requester.UserToken });
            }
        }
        public void RemoveRequest(RequestInfo requestInfo)
        {
            lock (requestInfos)
            {
                requestInfos.Remove(requestInfo);
                requestInfo.Requester.Connection.Send("main", new ConnectRequestInvalidatedPacket() { UserToken = requestInfo.Receiver.UserToken });
                requestInfo.Receiver.Connection.Send("main", new ConnectRequestInvalidatedPacket() { UserToken = requestInfo.Requester.UserToken });
            }
        }
        public void Clear()
        {
            lock (requestInfos)
                requestInfos.Clear();
        }
        public void RemoveAll(UserInfo userInfo)
        {
            lock(requestInfos)
                requestInfos.RemoveAll(x => x.Receiver == userInfo || x.Requester == userInfo);
        }
        public void Remove(UserInfo requester, UserInfo receiver)
        {
            lock (requestInfos)
                requestInfos.RemoveAll(x => x.Requester == requester && x.Receiver == receiver);
        }
        public RequestInfo? Find(UserInfo requester, UserInfo receiver)
        {
            lock (requestInfos)
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
