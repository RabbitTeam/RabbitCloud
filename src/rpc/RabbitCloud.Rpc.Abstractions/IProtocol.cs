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
        public EndPoint EndPoint { get; set; }
        public ServiceKey ServiceKey { get; set; }
    }

    public class ReferContext : ProtocolContext
    {
    }

    public class ExportContext : ProtocolContext
    {
        public ICaller Caller { get; set; }
    }
}