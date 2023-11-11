using BlessingStudio.WonderNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlessingStudio.Wrap.Server
{
    public class UserInfo
    {
        public string UserToken { get; set; } = "";
        public Connection Connection{ get; set; }
        public override bool Equals(object? obj)
        {
            if(obj is UserInfo info)
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
            if(left == null && right == null) return true;
            if(left == null || right == null) return false;
            return left.Equals(right);
        }
        public static bool operator !=(UserInfo? left, UserInfo? right)
        {
            return !(left == right);
        }
    }
}
