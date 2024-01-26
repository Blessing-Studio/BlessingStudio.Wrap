using System.Net;
using BlessingStudio.WonderNetwork.Interfaces;

namespace BlessingStudio.Wrap;

public sealed class UserInfo {
    public IPEndPoint IP { get; set; }

    public string UserToken { get; init; }

    public IConnection Connection { get; set; }

    public override bool Equals(object? obj) {
        return obj is UserInfo info && info.UserToken == UserToken;
    }

    public override int GetHashCode() {
        return UserToken.GetHashCode();
    }

    public static bool operator !=(UserInfo? left, UserInfo? right) => !(left == right);

    public static bool operator ==(UserInfo? left, UserInfo? right) => Equals(left, right);
}