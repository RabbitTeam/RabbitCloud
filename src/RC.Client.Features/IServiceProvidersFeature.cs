using System;

namespace Rabbit.Cloud.Client.Features
{
    public interface IServiceProvidersFeature
    {
        IServiceProvider RequestServices { get; set; }
    }
}