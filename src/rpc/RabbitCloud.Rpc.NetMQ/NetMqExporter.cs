using NetMQ;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.NetMQ.Internal;
using System;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqExporter : IExporter, IDisposable
    {
        #region Field

        private readonly ICaller _caller;
        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private readonly Action _disposable;

        #endregion Field

        #region Constructor

        public NetMqExporter(ICaller caller, IPEndPoint ipEndPoint, IRouterSocketFactory routerSocketFactory, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, Action disposable)
        {
            _caller = caller;
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _disposable = disposable;

            routerSocketFactory.OpenSocket(ipEndPoint, ReceiveReady);
        }

        #endregion Constructor

        #region Implementation of IExporter

        public ICaller Export()
        {
            return _caller;
        }

        #endregion Implementation of IExporter

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _disposable();
        }

        #endregion IDisposable

        #region Private Method

        private async void ReceiveReady(NetMQSocket socket)
        {
            //读取来自客户端的消息
            var requestMessage = socket.ReceiveMultipartMessage();

            //得到客户端消息主题
            var data = requestMessage.Last.Buffer;
            //得到客户端请求
            var request = _requestFormatter.InputFormatter.Format(data);

            //执行调用
            var response = await _caller.CallAsync(request);

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
    }
}