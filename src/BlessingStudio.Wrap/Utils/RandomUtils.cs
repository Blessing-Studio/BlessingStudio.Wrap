using System.Collections;
using System.Net.NetworkInformation;
using System.Net;

namespace BlessingStudio.Wrap.Utils;

public static class RandomUtils
{
    public static int GetRandomPort()
    {
        IList HasUsedPort = SocketUtils.PortIsUsed();
        int port = 0;
        bool IsRandomOk = true;
        Random random = new((int)DateTime.Now.Ticks);
        while (IsRandomOk)
        {
            port = random.Next(1024, 65535);
            IsRandomOk = HasUsedPort.Contains(port);
        }
        return port;
    }
}
