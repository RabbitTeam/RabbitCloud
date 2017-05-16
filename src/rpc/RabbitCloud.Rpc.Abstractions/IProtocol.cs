using RabbitCloud.Abstractions;
using System.Net;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IProtocol
    {
        IExporter Export(ExportContext context);

        ICaller Refer(ReferContext context);
    }

    public abstract class ProtocolContext
    {
        public ServiceDescriptor Descriptor { get; set; }
        public EndPoint EndPoint { get; set; }
    }

    public class ReferContext : ProtocolContext
    {
    }

    public class ExportContext : ProtocolContext
    {
    }
}