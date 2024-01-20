using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap
{
    public class UserManager : IEnumerable<UserInfo>
    {
        private List<UserInfo> users = new List<UserInfo>();
        public ReadOnlyCollection<UserInfo> Users
        {
            get
            {
                return users.AsReadOnly();
            }
        }
        public void AddNewUser(Connection connection, string userToken, IPEndPoint ip)
        {
            users.Add(new UserInfo()
            {
                Connection = connection,
                UserToken = userToken,
                IP = ip
            });
            connection.Disposed += e =>
            {
                RemoveUser(userToken);
            };
        }
        public void RemoveUser(Connection connection)
        {
            users.RemoveAll(x => x.Connection == connection);
        }
        public void RemoveUser(string userToken)
        {
            users.RemoveAll(x => x.UserToken == userToken );
        }
        public UserInfo? Find(string userToken)
        {
            foreach (UserInfo userInfo in users)
            {
                if(userInfo.UserToken == userToken) return userInfo;
            }
            return null;
        }
        public UserInfo? Find(IConnection connection)
        {
            foreach (UserInfo userInfo in users)
            {
                if (userInfo.Connection == connection) return userInfo;
            }
            return null;
        }
        public void DisconnectAll()
        {
            foreach (UserInfo user in users)
            {
                user.Connection.Dispose();
            }
            users.Clear();
        }

        public IEnumerator<UserInfo> GetEnumerator()
        {
            return ((IEnumerable<UserInfo>)Users).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Users).GetEnumerator();
        }
    }
}
