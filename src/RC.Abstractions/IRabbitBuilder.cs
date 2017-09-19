using Microsoft.Extensions.DependencyInjection;

namespace RC.Abstractions
{
    public interface IRabbitBuilder
    {
        IServiceCollection Services { get; }
    }
}