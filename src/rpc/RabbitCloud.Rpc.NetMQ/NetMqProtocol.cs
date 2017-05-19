using NetMQ;
using NetMQ.Sockets;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.NetMQ.Internal;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqProtocol : IProtocol, IDisposable
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

        #region Private Method

        private async void ReceiveReady(RouterSocket socket)
        {
            //读取来自客户端的消息
            var requestMessage = socket.ReceiveMultipartMessage();

            //得到客户端消息主题
            var data = requestMessage.Last.Buffer;
            //得到客户端请求
            var request = _requestFormatter.InputFormatter.Format(data);

            var protocolKey = GetProtocolKey(socket, request);
            _exporters.TryGetValue(protocolKey, out Lazy<IExporter> exporterLazy);
            var caller = exporterLazy.Value.Export();

            //执行调用
            var response = await caller.CallAsync(request);

            //格式化响应消息
            data = _responseFormatter.OutputFormatter.Format(response);

            //构建响应消息
            var responseMessage = new NetMQMessage();
            responseMessage.Append(requestMessage.First);
            responseMessage.AppendEmptyFrame();
            responseMessage.Append(data);

            //发送响应消息
            socket.SendMultipartMessage(responseMessage);
        }

        #endregion Private Method

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
            _exporters.Clear();
            _responseSocketFactory?.Dispose();
            _netMqPollerHolder?.Dispose();
        }

        #endregion IDisposable
    }
}