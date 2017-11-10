using Grpc.Core;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Server
{
    public class ServerServiceDefinitionProviderContext
    {
        public IList<ServerServiceDefinition> Results { get; } = new List<ServerServiceDefinition>();
    }

    public interface IServerServiceDefinitionProvider
    {
        int Order { get; }

        void OnProvidersExecuting(ServerServiceDefinitionProviderContext context);

        void OnProvidersExecuted(ServerServiceDefinitionProviderContext context);
    }
}