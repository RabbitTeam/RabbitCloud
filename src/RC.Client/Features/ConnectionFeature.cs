using System.Net;

namespace Rabbit.Cloud.Client.Features
{
    public class ConnectionFeature : IConnectionFeature
    {
        #region Implementation of IConnectionFeature

        public IPAddress RemoteIpAddress { get; set; }
        public IPAddress LocalIpAddress { get; set; }
        public int RemotePort { get; set; }
        public int LocalPort { get; set; }

        #endregion Implementation of IConnectionFeature
    }
}