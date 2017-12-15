using Rabbit.Cloud.Application.Features;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitRequest
    {
        IRabbitContext RabbitContext { get; }
        ServiceUrl Url { get; set; }
        object Request { get; set; }
    }
}