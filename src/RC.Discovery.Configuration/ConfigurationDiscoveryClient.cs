using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Configuration.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Discovery.Configuration
{
    public class ConfigurationDiscoveryClient : IDiscoveryClient
    {
        private readonly ILogger<ConfigurationDiscoveryClient> _logger;
        private readonly IConfiguration _configuration;

        private IReadOnlyCollection<IServiceInstance> _serviceInstances;

        public ConfigurationDiscoveryClient(ILogger<ConfigurationDiscoveryClient> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            void Init()
            {
                _serviceInstances = GetServiceInstances().ToArray();
                Services = _serviceInstances.Select(i => i.ServiceId).ToArray();

                _configuration.GetReloadToken().RegisterChangeCallback(s =>
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("service instances changed.");

                    Init();
                }, null);
            }
            Init();
        }

        #region Implementation of IDisposable

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion Implementation of IDisposable

        #region Implementation of IDiscoveryClient

        public string Description => "Rabbit Cloud Configuration Client";

        /// <inheritdoc />
        /// <summary>
        /// all serviceId
        /// </summary>
        public IReadOnlyCollection<string> Services { get; private set; }

        public IReadOnlyCollection<IServiceInstance> GetInstances(string serviceId)
        {
            return _serviceInstances;
        }

        #endregion Implementation of IDiscoveryClient

        private IEnumerable<IServiceInstance> GetServiceInstances()
        {
            return _configuration.GetChildren().Select(section => section.Get<ServiceInstance>());
        }
    }
}