using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Interfaces;
using System.Collections.ObjectModel;
using System.Net;

namespace BlessingStudio.Wrap.Interfaces
{
    public interface IUserManager
    {
        bool IsDisposed { get; }
        IReadOnlyCollection<UserInfo> Users { get; }

        void AddNewUser(IConnection connection, string userToken, IPEndPoint ip);
        void Close();
        void DisconnectAll();
        void Dispose();
        UserInfo? Find(IConnection connection);
        UserInfo? Find(string userToken);
        IEnumerator<UserInfo> GetEnumerator();
        void RemoveUser(IConnection connection);
        void RemoveUser(string userToken);
    }
}