using Grpc.Core;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Server
{
    public interface IServerServiceDefinitionProvider
    {
        IEnumerable<ServerServiceDefinition> GetDefinitions();
    }
}