using System;

namespace RabbitCloud.Rpc.Abstractions.Features
{
    public interface IServiceProvidersFeature
    {
        IServiceProvider RequestServices { get; set; }
    }

    public class ServiceProvidersFeature : IServiceProvidersFeature
    {
        public IServiceProvider RequestServices { get; set; }
    }
}