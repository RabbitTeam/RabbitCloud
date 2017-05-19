using RabbitCloud.Rpc.Abstractions;
using System;

namespace RabbitCloud.Rpc.NetMQ
{
    public class NetMqExporter : IExporter, IDisposable
    {
        #region Field

        private readonly ICaller _caller;
        private readonly Action _disposable;

        #endregion Field

        #region Constructor

        public NetMqExporter(ICaller caller, Action disposable)
        {
            _caller = caller;
            _disposable = disposable;
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
    }
}