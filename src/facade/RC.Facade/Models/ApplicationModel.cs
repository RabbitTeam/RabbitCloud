using Rabbit.Cloud.Facade.Abstractions.Filters;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Models
{
    public class ApplicationModel
    {
        public ApplicationModel()
        {
            Services = new List<ServiceModel>();
            Filters = new List<IFilterMetadata>();
        }

        public IList<ServiceModel> Services { get; }

        public IList<IFilterMetadata> Filters { get; }
    }
}