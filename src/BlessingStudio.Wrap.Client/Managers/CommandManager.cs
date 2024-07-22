using BlessingStudio.Wrap.Client.Commands;

namespace BlessingStudio.Wrap.Client.Managers;

public class CommandManager
{
    public List<ICommandExecuter> Executors { get; private set; } = new();
    public List<ICommandTabCompleter> TabCompleters { get; private set; } = new();
    public void RegisterCommandExecuter(ICommandExecuter executer)
    {
        Executors.Add(executer);
    }

    public void RegisterCommandTabCompleter(ICommandTabCompleter completer)
    {
        TabCompleters.Add(completer);
    }

    public ICommandExecuter? FindExecuter(string name)
    {
        return Executors.Find(x => x.GetName().Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public ICommandTabCompleter? FindTabCompleter(string name)
    {
        return TabCompleters.Find(x => x.GetName().Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public IList<string> Complete(string commanmd, string[] args)
    {
        ICommandTabCompleter? completer = FindTabCompleter(commanmd);
        if (completer == null) return new List<string>();
        return completer.OnComplete(args);
    }

    public IList<string> Complete(string commandLine)
    {
        string name = GetCommandName(commandLine);
        string[] args = GetCommandArgs(commandLine);
        return Complete(name, args);
    }

    public void Execute(string command, string[] args)
    {
        ICommandExecuter? executer = FindExecuter(command);
        if (executer == null)
        {
            Console.WriteLine($"未找到命令{command}");
            return;
        }
        try
        {
            executer.OnExecute(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"在执行命令{command}时发生异常 {ex.GetType().FullName} {ex.Message}");
        }
    }

    public void Execute(string commandLine)
    {
        string command = GetCommandName(commandLine);
        string[] args = GetCommandArgs(commandLine);
        Execute(command, args);
    }

    public static string GetCommandName(string commandLine)
    {
        return commandLine.Split(' ').First();
    }

    public static string[] GetCommandArgs(string commandLine)
    {
        List<string> strings = commandLine.Split(' ').ToList();
        strings.RemoveAt(0);
        return strings.ToArray();
    }
}
