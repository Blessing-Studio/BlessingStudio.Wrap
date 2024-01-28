using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Events;
using BlessingStudio.WonderNetwork.Interfaces;
using System.Collections;
using System.Collections.ObjectModel;
using System.Net;

namespace BlessingStudio.Wrap.Managers;

public class UserManager : IEnumerable<UserInfo>, IDisposable
{
    private readonly List<UserInfo> users = new();
    public bool IsDisposed { get; private set; } = false;
    public ReadOnlyCollection<UserInfo> Users
    {
        get
        {
            CheckDisposed();
            return users.AsReadOnly();
        }
    }
    ~UserManager()
    {
        Close();
    }
    public void AddNewUser(Connection connection, string userToken, IPEndPoint ip)
    {
        CheckDisposed();
        lock (users)
        {
            users.Add(new UserInfo()
            {
                Connection = connection,
                UserToken = userToken,
                IP = ip
            });
            connection.AddHandler((DisposedEvent _) =>
            {
                RemoveUser(userToken);
            });
        }
    }
    public void RemoveUser(Connection connection)
    {
        CheckDisposed();
        lock (users)
            users.RemoveAll(x => x.Connection == connection);
    }
    public void RemoveUser(string userToken)
    {
        CheckDisposed();
        lock (users)
            users.RemoveAll(x => x.UserToken == userToken);
    }
    public UserInfo? Find(string userToken)
    {
        CheckDisposed();
        lock (users)
        {
            foreach (UserInfo userInfo in users)
            {
                if (userInfo.UserToken == userToken) return userInfo;
            }
        }
        return null;
    }
    public UserInfo? Find(IConnection connection)
    {
        CheckDisposed();
        lock (users)
        {
            foreach (UserInfo userInfo in users)
            {
                if (userInfo.Connection == connection) return userInfo;
            }
        }
        return null;
    }
    public void DisconnectAll()
    {
        CheckDisposed();
        foreach (UserInfo user in users)
        {
            user.Connection.Dispose();
        }
        users.Clear();
    }

    public IEnumerator<UserInfo> GetEnumerator()
    {
        CheckDisposed();
        return ((IEnumerable<UserInfo>)Users).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        CheckDisposed();
        return ((IEnumerable)Users).GetEnumerator();
    }

    public void Close()
    {
        if (!IsDisposed)
        {
            DisconnectAll();
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
    public void Dispose()
    {
        Close();
    }
    private void CheckDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
