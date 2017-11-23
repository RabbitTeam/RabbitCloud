using System;

namespace Rabbit.Cloud.Application.Features
{
    public interface IServiceProvidersFeature
    {
        IServiceProvider RequestServices { get; set; }
    }
}