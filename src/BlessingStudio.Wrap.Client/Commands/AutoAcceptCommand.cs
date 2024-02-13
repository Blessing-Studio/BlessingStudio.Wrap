
using BlessingStudio.Wrap.Interfaces;
using ConsoleInteractive;

namespace BlessingStudio.Wrap.Client.Commands;

public class AutoAcceptCommand : CommandBase
{
    private IWrapClient Client { get; }
    public bool Enabled { get; set; } = false;
    public AutoAcceptCommand(IWrapClient client)
    {
        Client = client;
        Client.NewRequest += Client_NewRequest;
    }

    private void Client_NewRequest(Events.NewRequestEvent @event)
    {
        if (Enabled)
        {
            Client.AcceptRequest(@event.RequestInfo);
        }
    }

    public override string GetName()
    {
        return "autoaccept";
    }

    public override IList<string> OnComplete(string[] args)
    {
        return new List<string>() { "true", "false" };
    }

    public override void OnExecute(string[] args)
    {
        if (args.Length != 1 && args.Length != 0)
        {
            ConsoleWriter.WriteLine($"参数错误 用法 {GetName()} <State>[true|false]");
            return;
        }

        if (args.Length == 0)
        {
            ConsoleWriter.WriteLine($"自动同意当前状态  {Enabled}");
            return;
        }

        if(args.Length == 1)
        {
            try
            {
                Enabled = bool.Parse(args[0]);
                ConsoleWriter.WriteLine($"成功将自动同意状态设置为  {Enabled}");
            }
            catch
            {
                ConsoleWriter.WriteLine($"参数错误 用法 {GetName()} <State>[true|false]");
            }
        }
    }
}
