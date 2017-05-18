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
        #region Field

        private readonly IRequestFormatter _requestFormatter;
        private readonly IResponseFormatter _responseFormatter;
        private readonly ConcurrentDictionary<long, TaskCompletionSource<IResponse>> _taskCompletionSources = new ConcurrentDictionary<long, TaskCompletionSource<IResponse>>();
        private readonly DealerSocket _dealerSocket;

        #endregion Field

        #region Constructor

        public NetMqCaller(IPEndPoint ipEndPoint, IRequestFormatter requestFormatter, IResponseFormatter responseFormatter, NetMqPollerHolder netMqPollerHolder)
        {
            _requestFormatter = requestFormatter;
            _responseFormatter = responseFormatter;
            _dealerSocket = new DealerSocket();
            _dealerSocket.Connect("tcp://" + ipEndPoint);
            _dealerSocket.ReceiveReady += ReceiveReady;
            netMqPollerHolder.GetPoller().Add(_dealerSocket);
        }

        #endregion Constructor

        #region Implementation of ICaller

        public async Task<IResponse> CallAsync(IRequest request)
        {
            //格式化请求对象
            var data = _requestFormatter.OutputFormatter.Format(request);

            //构建请求消息
            var requestMessage = new NetMQMessage();
            requestMessage.AppendEmptyFrame();
            requestMessage.Append(data);

            //等待器
            var taskCompletionSource = new TaskCompletionSource<IResponse>();
            _taskCompletionSources.TryAdd(request.RequestId, taskCompletionSource);

            try
            {
                _dealerSocket.SendMultipartMessage(requestMessage);
                return await taskCompletionSource.Task;
            }
            catch
            {
                //请求失败从队列中移除
                _taskCompletionSources.TryRemove(request.RequestId, out taskCompletionSource);
                throw;
            }
        }

        #endregion Implementation of ICaller

        #region Private Method

        private void ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var message = e.Socket.ReceiveMultipartMessage();
            var data = message.Last.Buffer;

            var response = _responseFormatter.InputFormatter.Format(data);
            if (_taskCompletionSources.TryRemove(response.RequestId, out TaskCompletionSource<IResponse> source))
                source.SetResult(response);
        }

        #endregion Private Method
    }
}