using System.Text;

namespace RabbitCloud.Rpc.Abstractions.Utils
{
    public class ProtocolUtils
    {
        public static string GetServiceKey(int port, string serviceName, string serviceVersion, string serviceGroup)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(serviceGroup))
            {
                builder.Append(serviceGroup);
                builder.Append("/");
            }
            builder.Append(serviceName);
            if (!string.IsNullOrEmpty(serviceVersion) && !"0.0.0".Equals(serviceVersion))
            {
                builder.Append(":");
                builder.Append(serviceVersion);
            }
            builder.Append(":");
            builder.Append(port);
            return builder.ToString();
        }
    }
}
