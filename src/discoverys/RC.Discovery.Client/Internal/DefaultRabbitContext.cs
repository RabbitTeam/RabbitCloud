using Microsoft.AspNetCore.Http.Features;
using Rabbit.Cloud.Discovery.Client.Features;
using RC.Discovery.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using IItemsFeature = Rabbit.Cloud.Discovery.Abstractions.Features.IItemsFeature;

namespace Rabbit.Cloud.Discovery.Client.Internal
{
    public class DefaultRabbitContext : RabbitContext
    {
        public DefaultRabbitContext()
        {
            Request = new DefaultRabbitRequest(this) { RequestMessage = new HttpRequestMessage() };
            Response = new DefaultRabbitResponse(this) { ResponseMessage = new HttpResponseMessage() };
            Features = new FeatureCollection();
            Features.Set<IItemsFeature>(new ItemsFeature());
        }

        #region Overrides of RabbitContext

        public override IFeatureCollection Features { get; }
        public override RabbitRequest Request { get; }
        public override RabbitResponse Response { get; }

        public override IDictionary<object, object> Items
        {
            get => Features.Get<IItemsFeature>().Items;
            set => Features.Get<IItemsFeature>().Items = value;
        }

        public override IServiceProvider RequestServices { get; set; }

        #endregion Overrides of RabbitContext
    }
}