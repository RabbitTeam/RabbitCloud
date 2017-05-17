using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.NetMQ.Internal;
using System.Collections.Concurrent;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqProtocol : IProtocol
    {
        private readonly ConcurrentDictionary<string, IExporter> _exporters = new ConcurrentDictionary<string, IExporter>();
        private readonly IResponseSocketFactory _responseSocketFactory;
        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private readonly NetMqPollerHolder _netMqPollerHolder;

        public NetMqProtocol(IResponseSocketFactory responseSocketFactory, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, NetMqPollerHolder netMqPollerHolder)
        {
            _responseSocketFactory = responseSocketFactory;
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _netMqPollerHolder = netMqPollerHolder;
        }

        #region Implementation of IProtocol

        public IExporter Export(ExportContext context)
        {
            return new NetMqExporter(context.Caller, (IPEndPoint)context.EndPoint, _responseSocketFactory,
                _requestFormatter, _responseFormatter, _netMqPollerHolder);
        }

        public ICaller Refer(ReferContext context)
        {
            return new NetMqCaller((IPEndPoint)context.EndPoint, _requestFormatter, _responseFormatter, _netMqPollerHolder);
        }

        #endregion Implementation of IProtocol
    }
}