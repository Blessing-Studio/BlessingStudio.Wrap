using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.Wrap.Protocol.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Client
{
    public class PeerManager
    {
        public UserManager UserManager { get; set; } = new();
        public Dictionary<string, long> KeepAliveData = new Dictionary<string, long>();
        public Thread KeepAliveThread;
        public CancellationTokenSource KeepAliveThreadCancellationTokenSource = new();
        public PeerManager()
        {
            KeepAliveThread = new((e) =>
            {
                CancellationToken cancellationToken = (CancellationToken)e!;
                while(true)
                {
                    if(cancellationToken.IsCancellationRequested) return;
                    Thread.Sleep(500);
                    lock(KeepAliveData)
                    {
                        foreach(var pair in KeepAliveData)
                        {
                            if(DateTimeOffset.Now.ToUnixTimeSeconds() - pair.Value > 10)
                            {
                                UserInfo info = UserManager.Find(pair.Key)!;
                                info.Connection.Send("main", new DisconnectPacket() { Reason = "You didn't send KeepAlivePacket in 10s"});
                                info.Connection.Dispose();
                            }
                        }
                    }
                }
            });
            KeepAliveThread.Start(KeepAliveThreadCancellationTokenSource.Token);
        }
        public void AddPeer(string token, Connection connection)
        {
            UserManager.AddNewUser(connection, token);
            connection.AddHandler((ChannelCreatedEvent e) =>
            {
                
            });
            connection.AddHandler((DisposedEvent e) =>
            {
                lock (KeepAliveData)
                {
                    KeepAliveData.Remove(token);
                }
            });
            connection.AddHandler((ReceivedObjectEvent e) =>
            {
                if(e.Object is KeepAlivePacket packet)
                {
                    lock (KeepAliveData)
                    {
                        KeepAliveData[token] = DateTimeOffset.Now.ToUnixTimeSeconds();
                    }
                }
            });
            lock(KeepAliveData)
            {
                KeepAliveData[token] = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
        public void RemovePeer(string token)
        {
            UserInfo info = UserManager.Find(token)!;
            info.Connection.Dispose();
        }
    }
}
