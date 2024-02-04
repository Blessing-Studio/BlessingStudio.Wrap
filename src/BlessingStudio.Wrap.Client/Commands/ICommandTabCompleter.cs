namespace BlessingStudio.Wrap.Client.Commands;

public interface ICommandTabCompleter
{
    string GetName();
    IList<string> OnComplete(string[] args);
}
