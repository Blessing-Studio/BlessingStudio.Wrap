namespace BlessingStudio.Wrap.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WrapServer wrapServer = new WrapServer();
            Thread.Sleep(2000);
            wrapServer.Start();
        }
    }
}