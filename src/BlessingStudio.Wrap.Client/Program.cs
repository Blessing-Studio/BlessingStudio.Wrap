using BlessingStudio.Wrap.Utils;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NAT类型");
            Console.WriteLine(StunUtils.GetNatType());
            IPEndPoint iPEndPoint = IPEndPoint.Parse("0.0.0.0");
            iPEndPoint.Port = new Random().Next(20000, 60000);
            Console.WriteLine(StunUtils.GetRemoteIP(iPEndPoint));
            WrapClient wrapClient = new WrapClient();
            Thread.Sleep(5000);
            wrapClient.Connect(Dns.GetHostAddresses("mcocet.top").FirstOrDefault()!);
            Task.Run(() =>
            {
                string token = Console.ReadLine()!;
                wrapClient.MakeRequest(token);
            });
            wrapClient.Start();
        }
    }
}