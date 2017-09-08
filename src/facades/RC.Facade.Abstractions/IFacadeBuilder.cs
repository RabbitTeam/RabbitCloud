using Microsoft.Extensions.DependencyInjection;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public interface IFacadeBuilder
    {
        IServiceCollection Services { get; }
    }
}