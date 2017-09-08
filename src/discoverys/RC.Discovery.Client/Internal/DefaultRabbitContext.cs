using Microsoft.AspNetCore.Http.Features;
using Rabbit.Cloud.Discovery.Client.Features;
using RC.Discovery.Client.Abstractions;
using System.Collections.Generic;
using IItemsFeature = Rabbit.Cloud.Discovery.Abstractions.Features.IItemsFeature;

namespace Rabbit.Cloud.Discovery.Client.Internal
{
    public class DefaultRabbitContext : RabbitContext
    {
        public DefaultRabbitContext()
        {
            Request = new DefaultRabbitRequest();
            Response = new DefaultRabbitResponse();
            Features = new FeatureCollection();
            Features.Set<IItemsFeature>(new ItemsFeature());
        }

        #region Overrides of RabbitContext

        public override IFeatureCollection Features { get; }

        public override IDictionary<object, object> Items
        {
            get => Features.Get<IItemsFeature>().Items;
            set => Features.Get<IItemsFeature>().Items = value;
        }

        #endregion Overrides of RabbitContext
    }
}