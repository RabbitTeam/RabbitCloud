using Rabbit.Cloud.Discovery.Abstractions.Features;
using System.Collections.Generic;

namespace Rabbit.Cloud.Discovery.Client.Features
{
    public class ItemsFeature : IItemsFeature
    {
        public ItemsFeature()
        {
            Items = new Dictionary<object, object>();
        }

        public IDictionary<object, object> Items { get; set; }
    }
}