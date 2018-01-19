using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions.Filters
{
    public interface IAsyncExceptionFilter : IFilterMetadata
    {
        Task OnExceptionAsync(ExceptionContext context);
    }
}