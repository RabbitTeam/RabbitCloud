using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Models;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade
{
    public class FacadeOptions
    {
        public FormatterCollection<IOutputFormatter> OutputFormatters { get; } = new FormatterCollection<IOutputFormatter>();
        public FormatterCollection<IInputFormatter> InputFormatters { get; } = new FormatterCollection<IInputFormatter>();

        public IList<IApplicationModelConvention> Conventions { get; } = new List<IApplicationModelConvention>();
        public FilterCollection Filters { get; } = new FilterCollection();
    }
}