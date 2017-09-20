using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Abstractions.Features;
using Rabbit.Cloud.Features;
using System;
using System.Net.Http;

namespace Rabbit.Cloud.Internal
{
    public class DefaultRabbitContext : RabbitContext
    {
        public DefaultRabbitContext()
        {
            Request = new DefaultRabbitRequest(this) { RequestMessage = new HttpRequestMessage() };
            Response = new DefaultRabbitResponse(this) { ResponseMessage = new HttpResponseMessage() };
            Features = new FeatureCollection();
        }

        #region Overrides of RabbitContext

        public override IFeatureCollection Features { get; }
        public override RabbitRequest Request { get; }
        public override RabbitResponse Response { get; }
        public override IServiceProvider RequestServices { get; set; }

        #endregion Overrides of RabbitContext
    }
}