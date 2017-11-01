using Rabbit.Cloud.Client.Features;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitRequest
    {
        ServiceUrl Url { get; set; }
    }
}