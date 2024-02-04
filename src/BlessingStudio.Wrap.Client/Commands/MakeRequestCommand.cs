using BlessingStudio.Wrap.Interfaces;
using ConsoleInteractive;

namespace BlessingStudio.Wrap.Client.Commands;

public class MakeRequestCommand : CommandBase
{
    public IWrapClient Client { get; }
    public MakeRequestCommand(IWrapClient client) {  Client = client; }
    public override string GetName()
    {
        return "makerequest";
    }

    public override IList<string> OnComplete(string[] args)
    {
        if(args.Length == 1)
            return new List<string> { Client.UserToken };
        return new List<string>();
    }

    public override void OnExecute(string[] args)
    {
        if(args.Length != 1)
        {
            ConsoleWriter.WriteLine($"参数错误 用法 {GetName()} <UserToken>");
            return;
        }

        Client.MakeRequest(args[0]);
        ConsoleWriter.WriteLine($"成功发出对 {args[0]} 的请求");
    }
}
