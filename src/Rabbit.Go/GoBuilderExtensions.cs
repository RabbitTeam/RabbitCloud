using Rabbit.Cloud.Application.Abstractions.Extensions;
using Rabbit.Cloud.Application.Middlewares;
using Rabbit.Go;

// ReSharper disable once CheckNamespace
namespace Rabbit.Cloud.Application.Abstractions
{
    public static class GoBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseGoClient(this IRabbitApplicationBuilder app)
        {
            return app
                    .UseMiddleware<RequestServicesContainerMiddleware>()
                    .UseMiddleware<GoMiddleware>();
        }
    }
}