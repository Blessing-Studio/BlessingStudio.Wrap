using BlessingStudio.Wrap.Utils;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Console.WriteLine("NAT类型");
            Console.WriteLine(StunUtils.GetNatType());
            WrapClient wrapClient = new WrapClient();
            Thread.Sleep(1000);
            wrapClient.Connect(new IPAddress(new byte[] { 8, 137, 84, 98 }));
            Task.Run(() =>
            {
                string token = Console.ReadLine()!;
                wrapClient.MakeRequest(token);
            });
            wrapClient.Start();
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}