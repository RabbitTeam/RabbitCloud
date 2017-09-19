using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public interface IOutputFormatter
    {
        bool CanWriteResult(OutputFormatterContext context);

        Task<OutputFormatterResult> WriteAsync(OutputFormatterContext context);
    }
}