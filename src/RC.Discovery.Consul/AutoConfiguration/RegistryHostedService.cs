using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul.Registry;
using Rabbit.Cloud.Discovery.Consul.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Consul.AutoConfiguration
{
    public class RegistryHostedService : IHostedService
    {
        private readonly IRegistryService<ConsulRegistration> _registryService;
        private readonly ConsulInstanceOptions _options;

        public RegistryHostedService(IRegistryService<ConsulRegistration> registryService, IOptions<ConsulInstanceOptions> options)
        {
            _registryService = registryService;
            _options = options.Value;
        }

        #region Implementation of IHostedService

        /// <inheritdoc />
        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _registryService.RegisterAsync(ConsulUtil.Create(_options));
        }

        /// <inheritdoc />
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion Implementation of IHostedService
    }
}