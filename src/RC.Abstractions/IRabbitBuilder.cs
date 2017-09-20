using Microsoft.Extensions.DependencyInjection;

namespace Rabbit.Cloud.Abstractions
{
    public interface IRabbitBuilder
    {
        IServiceCollection Services { get; }
    }
}