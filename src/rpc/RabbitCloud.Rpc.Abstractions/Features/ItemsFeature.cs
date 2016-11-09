using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions.Features
{
    public interface IItemsFeature
    {
        IDictionary<object, object> Items { get; set; }
    }

    public class ItemsFeature : IItemsFeature
    {
        public ItemsFeature()
        {
            Items = new Dictionary<object, object>();
        }

        public IDictionary<object, object> Items { get; set; }
    }
}