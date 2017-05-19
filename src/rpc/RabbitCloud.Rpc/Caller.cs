using RabbitCloud.Rpc.Abstractions;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc
{
    public abstract class Caller : ICaller, IDisposable
    {
        #region Field

        private bool _isDispose;

        #endregion Field

        #region Implementation of ICaller

        public bool IsAvailable { get; private set; } = true;

        public Task<IResponse> CallAsync(IRequest request)
        {
            if (_isDispose)
                throw new ObjectDisposedException("caller is Dispose.");

            return DoCallAsync(request);
        }

        #endregion Implementation of ICaller

        protected abstract Task<IResponse> DoCallAsync(IRequest request);

        /// <summary>
        /// 释放资源。
        /// </summary>
        protected abstract void DoDispose();

        #region IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_isDispose)
                return;

            _isDispose = true;
            IsAvailable = false;

            DoDispose();
        }

        #endregion IDisposable
    }
}