using System.Net;

namespace Rabbit.Cloud.Client.Features
{
    public interface IConnectionFeature
    {
        IPAddress RemoteIpAddress { get; set; }
        IPAddress LocalIpAddress { get; set; }
        int RemotePort { get; set; }
        int LocalPort { get; set; }
    }
}