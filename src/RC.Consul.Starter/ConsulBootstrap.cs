using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Discovery.Abstractions;
using Rabbit.Cloud.Discovery.Consul;
using Rabbit.Cloud.Discovery.Consul.Registry;
using Rabbit.Cloud.Discovery.Consul.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Consul.Starter
{
    internal class RegistryInstanceService : IHostedService
    {
        private readonly IRegistryService<ConsulRegistration> _registryService;
        private readonly ConsulInstanceOptions _options;

        public RegistryInstanceService(IRegistryService<ConsulRegistration> registryService, IOptions<ConsulInstanceOptions> options)
        {
            _registryService = registryService;
            _options = options.Value;
        }

        #region Implementation of IHostedService

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _registryService.RegisterAsync(ConsulUtil.Create(_options));
        }

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

    public static class ConsulBootstrap
    {
        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var section = ctx.Configuration.GetSection("RabbitCloud:Consul");

                    if (section == null)
                        return;

                    var client = section.GetSection("Client");
                    var instance = section.GetSection("Instance");

                    if (client == null)
                        return;

                    services
                        .Configure<ConsulOptions>(client)
                        .AddConsulRegistry();

                    if (instance != null)
                        services
                        .Configure<ConsulInstanceOptions>(instance)
                        .Configure<ConsulInstanceOptions>(options =>
                            {
                                if (string.IsNullOrEmpty(options.InstanceId))
                                    options.InstanceId = $"{options.Host}_{options.Port}";
                                if (string.IsNullOrEmpty(options.HealthCheckInterval))
                                    options.HealthCheckInterval = "10s";
                            })
                        .AddConsulDiscovery()
                        .AddSingleton<IHostedService, RegistryInstanceService>();
                });
        }
    }
}