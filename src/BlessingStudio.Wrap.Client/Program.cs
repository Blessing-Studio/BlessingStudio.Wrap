using BlessingStudio.Wrap.Utils;
using System.Net;
using System.Net.Sockets;

namespace BlessingStudio.Wrap.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("NAT类型");
            Console.WriteLine(await StunUtils.GetNatTypeAsync());
            IPEndPoint iPEndPoint = IPEndPoint.Parse("0.0.0.0");
            iPEndPoint.Port = new Random().Next(20000, 60000);
            while (true)
            {
                Console.WriteLine(await StunUtils.GetRemoteIPAsync(iPEndPoint));
            }
        }
    }
}