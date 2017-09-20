using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;

namespace Rabbit.Cloud.Internal
{
    internal class RabbitBuilder : IRabbitBuilder
    {
        public RabbitBuilder(IServiceCollection services)
        {
            Services = services;
        }

        #region Implementation of IRabbitBuilder

        public IServiceCollection Services { get; }

        #endregion Implementation of IRabbitBuilder
    }
}