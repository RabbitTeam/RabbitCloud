using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public interface IAsyncExceptionFilter : IFilterMetadata
    {
        Task OnExceptionAsync(ExceptionContext context);
    }
}