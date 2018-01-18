using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;

namespace Rabbit.Cloud.Client.Features
{
    public class RabbitClientFeature : IRabbitClientFeature
    {
        #region Implementation of IRabbitClientFeature

        public ServiceRequestOptions RequestOptions { get; set; }

        #endregion Implementation of IRabbitClientFeature
    }
}