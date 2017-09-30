using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Features
{
    public class ItemsFeature : IItemsFeature
    {
        #region Implementation of IItemsFeature

        public IDictionary<object, object> Items { get; set; }

        #endregion Implementation of IItemsFeature
    }
}