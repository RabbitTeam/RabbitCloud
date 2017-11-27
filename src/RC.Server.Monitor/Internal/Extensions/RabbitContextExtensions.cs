using Rabbit.Cloud.Application.Abstractions;

namespace Rabbit.Cloud.Server.Monitor.Internal.Extensions
{
    internal static class RabbitContextExtensions
    {
        public static string GetServiceId(this IRabbitContext context)
        {
            var url = context.Request.Url;
            var serviceId = $"{url.Host}_{url.Port}_{url.Path}";

            return serviceId;
        }
    }
}