using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using RabbitCloud.Abstractions;
using RabbitCloud.Abstractions.Exceptions;
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
        private readonly ILogger<NetMqProtocol> _logger;

        public NetMqProtocol(IRouterSocketFactory responseSocketFactory, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, NetMqPollerHolder netMqPollerHolder, ILogger<NetMqProtocol> logger)
        {
            _responseSocketFactory = responseSocketFactory;
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _netMqPollerHolder = netMqPollerHolder;
            _logger = logger;
        }

        #region Implementation of IProtocol

        public IExporter Export(ExportContext context)
        {
            var protocolKey = GetProtocolKey(context);
            return _exporters.GetOrAdd(protocolKey, new Lazy<IExporter>(() =>
                {
                    var exporter = new NetMqExporter(context.Caller, () => _exporters.TryRemove(protocolKey, out Lazy<IExporter> _));

                    var ipEndPoint = (IPEndPoint)context.EndPoint;
                    _responseSocketFactory.OpenSocket<RouterSocket>("tcp", ipEndPoint, ReceiveReady);

                    return exporter;
                }))
                .Value;
        }

        public ICaller Refer(ReferContext context)
        {
            return new NetMqCaller(context.ServiceKey, (IPEndPoint)context.EndPoint, _requestFormatter, _responseFormatter, _netMqPollerHolder);
        }

        #endregion Implementation of IProtocol

        #region Private Method

        private async void ReceiveReady(RouterSocket socket)
        {
            //读取来自客户端的消息
            var message = socket.ReceiveMultipartMessage();

            //得到客户端消息主题
            var data = message.Last.Buffer;
            //得到客户端请求
            var request = _requestFormatter.InputFormatter.Format(data);

            IResponse response;
            var protocolKey = GetProtocolKey(socket, request);
            if (!_exporters.TryGetValue(protocolKey, out Lazy<IExporter> exporterLazy))
            {
                response = new Response(request)
                {
                    Exception = new RabbitServiceException($"can not get exporter protocolKey: '{protocolKey}',requestId: {request.RequestId}")
                };
                _logger.LogError($"can not get exporter protocolKey: '{protocolKey}',requestId: {request.RequestId}");
            }
            else
            {
                var caller = exporterLazy.Value.Export();
                //执行调用
                response = await caller.CallAsync(request);
            }

            //格式化响应消息
            data = _responseFormatter.OutputFormatter.Format(response);

            var identity = message.First;
            //构建响应消息
            message.Clear();
            message.Append(identity);
            message.AppendEmptyFrame();
            message.Append(data);

            //发送响应消息
            socket.SendMultipartMessage(message);
        }

        private static string GetProtocolKey(ProtocolContext context)
        {
            var ipEndPoint = (IPEndPoint)context.EndPoint;
            return GetProtocolKey(ipEndPoint, context.ServiceKey);
        }

        private static string GetProtocolKey(NetMQSocket socket, IRequest request)
        {
            var endpoint = socket.Options.LastEndpoint;
            var host = endpoint.Remove(0, 6/*tcp://*/);

            return GetProtocolKey(host, request.GetServiceKey());
        }

        private static string GetProtocolKey(IPEndPoint ipEndPoint, ServiceKey serviceKey)
        {
            return GetProtocolKey(ipEndPoint.Address.ToString(), ipEndPoint.Port, serviceKey);
        }

        private static string GetProtocolKey(string ip, int port, ServiceKey serviceKey)
        {
            return GetProtocolKey(ip + ":" + port, serviceKey);
        }

        private static string GetProtocolKey(string host, ServiceKey serviceKey)
        {
            return $"netmq://{host}/{serviceKey}";
        }

        #endregion Private Method

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var exporter in _exporters.Values)
                (exporter.Value as IDisposable)?.Dispose();

            _netMqPollerHolder?.Dispose();
        }

        #endregion IDisposable
    }
}