using BlessingStudio.Wrap.Client;
using BlessingStudio.Wrap.Utils;
using System.Net;

AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
Console.WriteLine("NAT类型");
Console.WriteLine(StunUtils.GetNatType());
WrapClient wrapClient = new();
Thread.Sleep(1000);
wrapClient.Connect(new IPAddress(new byte[] { 8, 137, 84, 98 }));
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

wrapClient.ExpectedDisconnect += e =>
{
    Console.WriteLine($"与{e.UserToken}断开连接\n原因 {e.Reason}");
};

wrapClient.UnexpectedDisconnect += e =>
{
    Console.WriteLine($"与{e.UserToken}断开连接");
};

wrapClient.LoginedSuccessfully += e =>
{
    Console.WriteLine("登录成功");
    Console.WriteLine($"UserToken   {e.UserToken}");
    Task.Run(() =>
    {
        string token = Console.ReadLine()!;
        wrapClient.MakeRequest(token);
    });
};

wrapClient.NewRequest += e =>
{
    wrapClient.AcceptRequest(e.RequestInfo);
};
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
wrapClient.Start();
void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Console.WriteLine(e.ExceptionObject.ToString());
}
