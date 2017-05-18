using NetMQ;
using System;

namespace RabbitCloud.Rpc.NetMQ.Internal
{
    public class NetMqPollerHolder : IDisposable
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

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _poller?.Dispose();
        }

        #endregion IDisposable
    }
}