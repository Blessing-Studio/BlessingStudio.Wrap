using BlessingStudio.WonderNetwork.Interfaces;
using System.Net;

namespace BlessingStudio.Wrap;

public sealed class UserInfo
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public IPEndPoint IP { get; set; }

    public string UserToken { get; init; }

    public IConnection Connection { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is UserInfo info && info.UserToken == UserToken;
    }

    public override int GetHashCode()
    {
        return UserToken.GetHashCode();
    }

    public static bool operator !=(UserInfo? left, UserInfo? right) => !(left == right);

    public static bool operator ==(UserInfo? left, UserInfo? right) => Equals(left, right);

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
}