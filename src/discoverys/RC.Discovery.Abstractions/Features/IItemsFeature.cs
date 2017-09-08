using System.Collections.Generic;

namespace Rabbit.Cloud.Discovery.Abstractions.Features
{
    public interface IItemsFeature
    {
        IDictionary<object, object> Items { get; set; }
    }
}
