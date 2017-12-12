using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions.Server;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Grpc.AutoConfiguration
{
    public class GrpcServerHostedService : IHostedService
    {
        private readonly IServerServiceDefinitionTable _serverServiceDefinitionTable;
        private readonly GrpcServerOptions _options;
        private global::Grpc.Core.Server _server;

        public GrpcServerHostedService(IOptions<GrpcServerOptions> options, IServerServiceDefinitionTableProvider serverServiceDefinitionTableProvider)
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
            _server = new global::Grpc.Core.Server
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
}