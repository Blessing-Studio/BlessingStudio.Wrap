
namespace BlessingStudio.Wrap.Client.Commands;

public abstract class CommandBase : ICommandExecuter, ICommandTabCompleter
{
    public abstract string GetName();
    public abstract IList<string> OnComplete(string[] args);
    public abstract void OnExecute(string[] args);
}
