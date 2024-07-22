
using BlessingStudio.Wrap.Interfaces;

namespace BlessingStudio.Wrap.Client.Commands;

public class KickCommand : CommandBase
{
    public IWrapClient Client { get; }
    public KickCommand(IWrapClient client) {  Client = client; }
    public override string GetName()
    {
        return "kick";
    }

    public override IList<string> OnComplete(string[] args)
    {
        if(args.Length == 1)
            return Client.PeerManager.UserManager.Users.Select(x => x.UserToken).ToList();
        return new List<string>();
    }

    public override void OnExecute(string[] args)
    {
        if (args.Length != 1 && args.Length != 2)
        {
            Console.WriteLine($"参数错误 用法 {GetName()} <UserToken> [Reason]");
            return;
        }

        if (Client.PeerManager.UserManager.Find(args[0]) is null)
        {
            Console.WriteLine("未找到用户");
            return;
        }

        if (args.Length == 1)
        {
            Client.PeerManager.RemovePeer(args[0], "被用户踢出");
            Console.WriteLine($"成功踢出了 {args[0]}");
        }
        else
        {
            Client.PeerManager.RemovePeer(args[0], args[1]);
            Console.WriteLine($"成功踢出了 {args[0]} 原因 {args[1]}");
        }
    }
}
