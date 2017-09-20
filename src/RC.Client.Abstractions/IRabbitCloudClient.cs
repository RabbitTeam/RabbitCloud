using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Abstractions.Features;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRabbitCloudClient
    {
        IFeatureCollection ServerFeatures { get; }
        IServiceProvider Services { get; }

        Task RequestAsync(RabbitContext rabbitContext);
    }
}