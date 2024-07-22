using BlessingStudio.Wrap.Interfaces;

namespace BlessingStudio.Wrap.Client.Commands;

public class AcceptRequestCommand : CommandBase
{
    public IWrapClient Client { get; }
    public AcceptRequestCommand(IWrapClient client) {  Client = client; }
    public override string GetName()
    {
        return "acceptrequest";
    }

    public override IList<string> OnComplete(string[] args)
    {
        if(args.Length == 1)
            return Client.Requests.Select(x => x.Requester).ToList();
        return new List<string>();
    }

    public override void OnExecute(string[] args)
    {
        if(args.Length != 1)
        {
            Console.WriteLine($"参数错误 用法 {GetName()} <UserToken>");
            return;
        }
        RequestInfo? requestInfo = Client.Requests.FirstOrDefault(x => x.Requester == args[0]);
        if(requestInfo == null)
        {
            Console.WriteLine("未找到请求");
            return;
        }
        Console.WriteLine($"同意了 {args[0]} 的请求");
        Client.AcceptRequest(requestInfo);
    }
}
