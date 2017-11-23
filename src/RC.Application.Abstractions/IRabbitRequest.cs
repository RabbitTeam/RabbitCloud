using Rabbit.Cloud.Application.Features;

namespace Rabbit.Cloud.Application.Abstractions
{
    public interface IRabbitRequest
    {
        ServiceUrl Url { get; set; }
    }
}