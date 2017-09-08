using Rabbit.Cloud.Facade.Abstractions.Formatters;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public class FacadeOptions
    {
        public FormatterCollection<IOutputFormatter> OutputFormatters { get; } = new FormatterCollection<IOutputFormatter>();
    }
}