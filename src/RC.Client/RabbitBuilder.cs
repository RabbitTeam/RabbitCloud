/*using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client.Abstractions;

namespace Rabbit.Cloud.Client
{
    public class RabbitBuilder : IRabbitBuilder
    {
        public RabbitBuilder(IServiceCollection services)
        {
            Services = services;
        }

        #region Implementation of IRabbitBuilder

        public IServiceCollection Services { get; }

        #endregion Implementation of IRabbitBuilder
    }

    public static class RabbitBuilderExtensions
    {
        public static IRabbitBuilder AddRabbitCloudCore(this IServiceCollection services)
        {
            return new RabbitBuilder(services);
        }
    }
}*/