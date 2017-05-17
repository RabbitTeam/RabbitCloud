using NetMQ;
using NetMQ.Sockets;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.NetMQ.Internal;
using System;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqExporter : IExporter, IDisposable
    {
        private readonly ICaller _caller;
        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private readonly Action _disposable;
        private readonly ResponseSocket _responseSocket;

        public NetMqExporter(ICaller caller, IPEndPoint endPoint, IResponseSocketFactory responseSocketFactory, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, NetMqPollerHolder netMqPollerHolder, Action disposable)
        {
            _caller = caller;
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _disposable = disposable;
            _responseSocket = responseSocketFactory.GetResponseSocket(endPoint);
            _responseSocket.ReceiveReady += _responseSocket_ReceiveReady;
            netMqPollerHolder.GetPoller().Add(_responseSocket);
        }

        private async void _responseSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var data = e.Socket.ReceiveFrameBytes();

            var request = _requestFormatter.InputFormatter.Format(data);

            var response = await _caller.CallAsync(request);

            data = _responseFormatter.OutputFormatter.Format(response);

            e.Socket.SendFrame(data);
        }

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
            _responseSocket?.Dispose();
        }

        #endregion IDisposable
    }
}