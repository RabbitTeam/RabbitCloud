using NetMQ;

namespace RabbitCloud.Rpc.NetMQ.Internal
{
    public class NetMqPollerHolder
    {
        private readonly NetMQPoller _poller = new NetMQPoller();

        public NetMQPoller GetPoller()
        {
            if (_poller.IsRunning)
                return _poller;
            lock (this)
            {
                if (!_poller.IsRunning)
                    _poller.RunAsync();
            }
            return _poller;
        }
    }
}