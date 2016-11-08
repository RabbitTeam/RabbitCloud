using RabbitCloud.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Utils.Extensions
{
    public static class UrlExtensions
    {
        public static async Task<IPEndPoint> TryGetIpEndPoint(this Url url)
        {
            try
            {
                return await url.GetIpEndPoint();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<IPEndPoint> GetIpEndPoint(this Url url)
        {
            var host = url.Host;
            IPAddress ipAddress;
            var port = url.Port;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                var addresses = await Dns.GetHostAddressesAsync(host);
                if (addresses.Any())
                    ipAddress = addresses.First();
            }
            if (ipAddress == null)
                throw new NotSupportedException($"根据Url:{url}，无法得到IPEndPoint。");
            return new IPEndPoint(ipAddress, port);
        }

        public static string GetServiceKey(this Url url)
        {
            return ProtocolUtils.GetServiceKey(url.Port, url.Path, url.Parameters.Get("version"), url.Parameters.Get("group"));
        }
    }
}