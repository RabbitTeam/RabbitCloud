using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Features
{
    public interface IItemsFeature
    {
        IDictionary<object, object> Items { get; set; }
    }
}