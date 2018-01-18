using Rabbit.Go.ApplicationModels;
using Rabbit.Go.Filters;
using System.Collections.Generic;

namespace Rabbit.Go
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