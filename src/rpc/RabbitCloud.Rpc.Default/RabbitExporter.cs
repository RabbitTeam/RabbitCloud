using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Internal;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitExporter : Exporter
    {
        public RabbitExporter(IProvider provider, Url url) : base(provider, url)
        {
        }
    }
}