using Rabbit.Cloud.Client.Abstractions;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public interface IFacadeBuilder
    {
        IRabbitBuilder RabbitBuilder { get; }
    }
}