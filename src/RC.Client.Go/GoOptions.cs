using Rabbit.Cloud.Client.Go.ApplicationModels;
using Rabbit.Cloud.Client.Go.Filters;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go
{
    public class GoOptions
    {
        public GoOptions()
        {
            DefaultScheme = "http";
            Filters = new FilterCollection();
            Conventions = new List<IApplicationModelConvention>();
        }

        public string DefaultScheme { get; set; }
        public FilterCollection Filters { get; }
        public IList<IApplicationModelConvention> Conventions { get; }
    }
}