using Consul;
using Microsoft.Extensions.Options;
using System;

namespace Rabbit.Cloud.Discovery.Consul
{
    public abstract class ConsulService : IDisposable
    {
        protected bool Disposed { get; private set; }
        public IConsulClient ConsulClient { get; private set; }

        protected ConsulService(IOptionsMonitor<ConsulOptions> consulOptionsMonitor)
        {
            ConsulClient = consulOptionsMonitor.CurrentValue.CreateClient();
            consulOptionsMonitor.OnChange(options =>
            {
                ConsulClient = options.CreateClient();
            });
        }

        #region IDisposable

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Disposed = true;
            ConsulClient?.Dispose();
        }

        #endregion IDisposable
    }
}