using RabbitCloud.Rpc.Abstractions;
using System;

namespace RabbitCloud.Rpc.Memory
{
    public class MemoryExporter : IExporter, IDisposable
    {
        private readonly ICaller _caller;
        private readonly Action _dispose;

        public MemoryExporter(ICaller caller, Action dispose)
        {
            _caller = caller;
            _dispose = dispose;
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
            _dispose();
        }

        #endregion IDisposable
    }
}