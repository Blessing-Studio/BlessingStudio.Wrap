using BlessingStudio.WonderNetwork;
using BlessingStudio.WonderNetwork.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap
{
    public class UserInfo
    {
        public string UserToken { get; set; } = "";
        public IConnection Connection { get; set; }
        public IPEndPoint IP {  get; set; }
        public override bool Equals(object? obj)
        {
            if (obj is UserInfo info)
            {
                return info.UserToken == UserToken;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return UserToken.GetHashCode();
        }
        public static bool operator ==(UserInfo? left, UserInfo? right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.UserToken == right.UserToken;
        }
        public static bool operator !=(UserInfo? left, UserInfo? right)
        {
            return !(left == right);
        }
    }
}
