using BlessingStudio.Wrap.Interfaces;
using BlessingStudio.Wrap.Utils;
using System.Net;
using Waher.Networking.UPnP;

namespace BlessingStudio.Wrap;

public class UPnP : IUPnPService
{
    public UPnPService UPnPService { get; private set; }
    public UPnP(UPnPService service) {  UPnPService = service; }
    public void AddPortMapping(IPAddress? NewRemoteHost, int NewExternalPort, IUPnPService.SocketProtocol NewProtocol, IPAddress NewInternalClient, int NewInternalPort, bool NewEnabled, string NewPortMappingDescription, TimeSpan NewLeaseDuration)
    {
        UPnPService.GetService().InvokeAsync("AddPortMapping", 3000,
                new KeyValuePair<string, object>("NewRemoteHost", (NewRemoteHost is null) ? GetExternalIPAddress().ToString() : NewRemoteHost.ToString()),
                new KeyValuePair<string, object>("NewExternalPort", NewExternalPort),
                new KeyValuePair<string, object>("NewProtocol", NewProtocol.ToString()),
                new KeyValuePair<string, object>("NewInternalPort", NewInternalPort),
                new KeyValuePair<string, object>("NewInternalClient", NewInternalClient.ToString()),
                new KeyValuePair<string, object>("NewEnabled", true),
                new KeyValuePair<string, object>("NewPortMappingDescription", NewPortMappingDescription),
                new KeyValuePair<string, object>("NewLeaseDuration", NewLeaseDuration.TotalSeconds)).GetAwaiter().GetResult();
    }

    public void DeletePortMapping(IPAddress? NewRemoteHost, int NewExternalPort, IUPnPService.SocketProtocol NewProtocol)
    {
        UPnPService.GetService().InvokeAsync("DeletePortMapping", 3000,
                new KeyValuePair<string, object>("NewRemoteHost", (NewRemoteHost is null) ? GetExternalIPAddress().ToString() : NewRemoteHost.ToString()),
                new KeyValuePair<string, object>("NewExternalPort", NewExternalPort),
                new KeyValuePair<string, object>("NewProtocol", NewProtocol.ToString())).GetAwaiter().GetResult();
    }

    public IPAddress GetExternalIPAddress()
    {
        string address = (string)UPnPService.GetService().InvokeAsync("GetExternalIPAddress", 3000).GetAwaiter().GetResult().Value["NewExternalIPAddress"];
        return IPAddress.Parse(address);
    }

    public UPnPService GetUPnPService()
    {
        return UPnPService;
    }
}
