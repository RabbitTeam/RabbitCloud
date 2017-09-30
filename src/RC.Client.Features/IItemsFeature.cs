using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Features
{
    public interface IItemsFeature
    {
        IDictionary<object, object> Items { get; set; }
    }
}