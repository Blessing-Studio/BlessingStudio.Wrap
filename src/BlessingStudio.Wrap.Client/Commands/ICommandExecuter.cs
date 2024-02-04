namespace BlessingStudio.Wrap.Client.Commands;

public interface ICommandExecuter
{
    string GetName();
    void OnExecute(string[] args);
}
