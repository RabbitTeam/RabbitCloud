using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using System;

namespace Rabbit.Cloud.Client.Features
{
    public class RabbitClientFeature : IRabbitClientFeature
    {
        #region Implementation of IRabbitClientFeature

        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
        public ServiceRequestOptions RequestOptions { get; set; }

        #endregion Implementation of IRabbitClientFeature
    }
}