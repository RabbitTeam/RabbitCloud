using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Abstractions.Formatters
{
    public interface IInputFormatter
    {
        bool CanWriteResult(InputFormatterCanWriteContext context);

        Task WriteAsync(InputFormatterWriteContext context);
    }
}