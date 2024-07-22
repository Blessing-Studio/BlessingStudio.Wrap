using BlessingStudio.Wrap.Interfaces;
using System.Net;

namespace BlessingStudio.Wrap.Client.Commands;

public class SetServerCommand : CommandBase
{
    public IWrapClient Client { get; }
    public SetServerCommand(IWrapClient client) { Client = client; }

    public override string GetName()
    {
        return "setserver";
    }

    public override IList<string> OnComplete(string[] args)
    {
        if (args.Length == 1)
            return new List<string> { Client.PeerManager.Server.ToString() };
        return new List<string>();
    }

    public override void OnExecute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("参数错误 用法 serserver <ServerIP>");
            return;
        }

        try
        {
            IPEndPoint iP = IPEndPoint.Parse(args[0]);
            Client.PeerManager.Server = iP;
            Console.WriteLine($"成功将IP更改为{iP}");
        }
        catch
        {
            Console.WriteLine("服务器ip格式不正确 应为x.x.x.x:xxxx");
            return;
        }
    }
}
