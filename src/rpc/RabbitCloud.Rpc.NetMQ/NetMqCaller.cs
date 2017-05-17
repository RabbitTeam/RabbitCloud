using NetMQ;
using NetMQ.Sockets;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.NetMQ.Internal;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqCaller : ICaller
    {
        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private readonly RequestSocket _requestSocket;
        private readonly ConcurrentDictionary<long, TaskCompletionSource<IResponse>> _taskCompletionSources = new ConcurrentDictionary<long, TaskCompletionSource<IResponse>>();
        public static readonly NetMQPoller NetMqPoller = new NetMQPoller();

        public NetMqCaller(IPEndPoint ipEndPoint, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, NetMqPollerHolder netMqPollerHolder)
        {
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _requestSocket = new RequestSocket();
            _requestSocket.Connect($"tcp://{ipEndPoint.Address}:{ipEndPoint.Port}");
            _requestSocket.ReceiveReady += _requestSocket_ReceiveReady;
            netMqPollerHolder.GetPoller().Add(_requestSocket);
        }

        private void _requestSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var data = e.Socket.ReceiveFrameBytes();
            var response = _responseFormatter.InputFormatter.Format(data);
            _taskCompletionSources.TryRemove(response.RequestId, out TaskCompletionSource<IResponse> source);
            source.SetResult(response);
        }

        #region Implementation of ICaller

        public Task<IResponse> CallAsync(IRequest request)
        {
            var data = _requestFormatter.OutputFormatter.Format(request);
            _requestSocket.SendFrame(data);
            var taskCompletionSource = new TaskCompletionSource<IResponse>();
            _taskCompletionSources.TryAdd(request.RequestId, taskCompletionSource);
            return taskCompletionSource.Task;
        }

        #endregion Implementation of ICaller
    }
}