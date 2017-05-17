using NetMQ;
using NetMQ.Sockets;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using System.Net;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqExporter : IExporter
    {
        private readonly ICaller _caller;
        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private ResponseSocket _responseSocket;

        public NetMqExporter(ICaller caller, IPEndPoint endPoint, IResponseSocketFactory responseSocketFactory, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter)
        {
            _caller = caller;
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _responseSocket = responseSocketFactory.GetResponseSocket(endPoint);
            _responseSocket.ReceiveReady += _responseSocket_ReceiveReady;
            NetMqCaller.NetMqPoller.Add(_responseSocket);
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
    }
}