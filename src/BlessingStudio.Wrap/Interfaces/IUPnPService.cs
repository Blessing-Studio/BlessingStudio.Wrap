using System.Net;
using System.Net.Sockets;
using Waher.Networking.UPnP;

namespace BlessingStudio.Wrap.Interfaces;

public interface IUPnPService
{
    UPnPService GetUPnPService();
    public enum SocketProtocol
    {
        TCP, UDP
    }
    void AddPortMapping(IPAddress? NewRemoteHost, int NewExternalPort, SocketProtocol NewProtocol, IPAddress NewInternalClient, int NewInternalPort, bool NewEnabled, string NewPortMappingDescription, TimeSpan NewLeaseDuration);
    void AddPortMapping(int NewExternalPort, SocketProtocol NewProtocol, int NewInternalPort, bool NewEnabled, string NewPortMappingDescription, TimeSpan NewLeaseDuration)
    {
        string localAddress = GetLocalIpAddress(AddressFamily.InterNetwork).First();
        AddPortMapping(null, NewExternalPort, NewProtocol, IPAddress.Parse(localAddress), NewInternalPort, NewEnabled, NewPortMappingDescription, NewLeaseDuration);
    }
    void AddPortMapping(int NewExternalPort, SocketProtocol NewProtocol, int NewInternalPort, bool NewEnabled, string NewPortMappingDescription)
    {
        AddPortMapping(NewExternalPort, NewProtocol, NewInternalPort, NewEnabled, NewPortMappingDescription, TimeSpan.Zero);
    }
    void AddPortMapping(int NewExternalPort, SocketProtocol NewProtocol, int NewInternalPort, string NewPortMappingDescription, TimeSpan NewLeaseDuration)
    {
        AddPortMapping(NewExternalPort, NewProtocol, NewInternalPort, true, NewPortMappingDescription, NewLeaseDuration);
    }
    void AddPortMapping(int NewExternalPort, SocketProtocol NewProtocol, int NewInternalPort, string NewPortMappingDescription)
    {
        AddPortMapping(NewExternalPort, NewProtocol, NewInternalPort, true, NewPortMappingDescription);
    }
    void AddPortMapping(int NewExternalPort, SocketProtocol NewProtocol, int NewInternalPort)
    {
        AddPortMapping(NewExternalPort, NewProtocol, NewInternalPort, "");
    }
    void DeletePortMapping(IPAddress? NewRemoteHost, int NewExternalPort, SocketProtocol NewProtocol);
    void DeletePortMapping(int NewExternalPort, SocketProtocol NewProtocol)
    {
        DeletePortMapping(null, NewExternalPort, NewProtocol);
    }
    IPAddress GetExternalIPAddress();
    /// <summary>
    /// 获取本机所有ip地址
    /// </summary>
    /// <returns>ip地址集合</returns>
    public static List<string> GetLocalIpAddress(AddressFamily? netType)
    {
        string hostName = Dns.GetHostName();                    //获取主机名称  
        IPAddress[] addresses = Dns.GetHostAddresses(hostName); //解析主机IP地址  

        List<string> IPList = new();
        if (netType is null)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                IPList.Add(addresses[i].ToString());
            }
        }
        else
        {
            //AddressFamily.InterNetwork表示此IP为IPv4,
            //AddressFamily.InterNetworkV6表示此地址为IPv6类型
            for (int i = 0; i < addresses.Length; i++)
            {
                if (addresses[i].AddressFamily == netType)
                {
                    IPList.Add(addresses[i].ToString());
                }
            }
        }
        return IPList;
}
}
