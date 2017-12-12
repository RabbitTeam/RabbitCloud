using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc;
using Rabbit.Cloud.Grpc.Abstractions.Server;
using System.Threading;
using System.Threading.Tasks;
using GoogleGrpc = Grpc.Core;

namespace Rabbit.Cloud.Server.Grpc.Starter
{
    public class GrpcServerOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class StartGrpcServerService : IHostedService
    {
        private readonly IServerServiceDefinitionTable _serverServiceDefinitionTable;
        private readonly GrpcServerOptions _options;
        private GoogleGrpc.Server _server;

        public StartGrpcServerService(IOptions<GrpcServerOptions> options, IServerServiceDefinitionTableProvider serverServiceDefinitionTableProvider)
        {
            _serverServiceDefinitionTable = serverServiceDefinitionTableProvider.ServerServiceDefinitionTable;
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
            _server = new GoogleGrpc.Server
            {
                Ports = { new ServerPort(_options.Host, _options.Port, ServerCredentials.Insecure) }
            };

            foreach (var definition in _serverServiceDefinitionTable)
            {
                _server.Services.Add(definition);
            }
            _server.Start();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _server == null ? Task.CompletedTask : _server.ShutdownAsync();
        }

        #endregion Implementation of IHostedService
    }

    public class GrpcBootstrap
    {
        public static int Priority => 10;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var grpConfiguration = ctx.Configuration.GetSection("RabbitCloud:Server:Grpc");
                    if (grpConfiguration == null)
                        return;
                    services
                        .Configure<GrpcServerOptions>(grpConfiguration)
                        .AddGrpcServer()
                        .AddServerGrpc()
                        .AddSingleton<IHostedService,StartGrpcServerService>();
                });
        }
    }
}