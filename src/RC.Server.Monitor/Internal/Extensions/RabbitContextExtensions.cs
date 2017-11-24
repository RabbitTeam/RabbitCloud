using Rabbit.Cloud.Application.Abstractions;

namespace Rabbit.Cloud.Server.Monitor.Internal.Extensions
{
    internal static class RabbitContextExtensions
    {
        public static string GetServiceId(this IRabbitContext context)
        {
            var serviceId = $"{context.Request.Url.Host.Replace(":", "_")}{context.Request.Url.Path}";

            return serviceId;
        }
    }
}