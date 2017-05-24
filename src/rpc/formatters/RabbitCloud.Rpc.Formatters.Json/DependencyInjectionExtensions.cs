using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Formatters.Json.Config;

namespace RabbitCloud.Rpc.Formatters.Json
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddJsonFormatter(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRequestFormatter, JsonRequestFormatter>()
                .AddSingleton<IResponseFormatter, JsonResponseFormatter>()
                .AddSingleton<IFormatterProvider, JsonFormatterProvider>();
        }
    }
}