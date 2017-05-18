using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.NetMQ.Internal;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqProtocol : IProtocol
    {
        private readonly ConcurrentDictionary<string, Lazy<IExporter>> _exporters = new ConcurrentDictionary<string, Lazy<IExporter>>();
        private readonly IRouterSocketFactory _responseSocketFactory;
        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private readonly NetMqPollerHolder _netMqPollerHolder;

        public NetMqProtocol(IRouterSocketFactory responseSocketFactory, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, NetMqPollerHolder netMqPollerHolder)
        {
            _responseSocketFactory = responseSocketFactory;
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _netMqPollerHolder = netMqPollerHolder;
        }

        #region Implementation of IProtocol

        public IExporter Export(ExportContext context)
        {
            var protocolKey = GetProtocolKey(context);
            return _exporters.GetOrAdd(protocolKey, new Lazy<IExporter>(() => new NetMqExporter(context.Caller, (IPEndPoint)context.EndPoint, _responseSocketFactory,
                    _requestFormatter, _responseFormatter, () => _exporters.TryRemove(protocolKey, out Lazy<IExporter> _))))
                .Value;
        }

        public ICaller Refer(ReferContext context)
        {
            return new NetMqCaller((IPEndPoint)context.EndPoint, _requestFormatter, _responseFormatter, _netMqPollerHolder);
        }

        #endregion Implementation of IProtocol

        #region Private Method

        private static string GetProtocolKey(ProtocolContext context)
        {
            var ipEndPoint = (IPEndPoint)context.EndPoint;
            var serviceKey = context.ServiceKey.ToString();

            return $"netmq://{ipEndPoint}/{serviceKey}";
        }

        #endregion Private Method
    }
}