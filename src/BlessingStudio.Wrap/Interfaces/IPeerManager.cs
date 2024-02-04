using BlessingStudio.WonderNetwork.Interfaces;
using System.Collections.Concurrent;
using System.Net;

namespace BlessingStudio.Wrap.Interfaces;

public interface IPeerManager
{
    List<string> IgnoredConnectionId { get; }
    bool IsDisposed { get; }
    ConcurrentDictionary<string, DateTimeOffset> KeepAliveData { get; }
    Thread KeepAliveThread { get; }
    CancellationTokenSource KeepAliveThreadCancellationTokenSource { get; }
    ushort Nextport { get; set; }
    ushort LastPort { get; }
    IPEndPoint Server { get; set; }
    IUserManager UserManager { get; }
    void AddPeer(string token, IConnection connection, IPEndPoint ip);
    void Close();
    void Dispose();
    void RemovePeer(string token);
}