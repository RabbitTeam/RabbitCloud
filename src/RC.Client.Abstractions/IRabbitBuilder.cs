using Microsoft.Extensions.DependencyInjection;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitBuilder
    {
        IServiceCollection Services { get; }
    }
}