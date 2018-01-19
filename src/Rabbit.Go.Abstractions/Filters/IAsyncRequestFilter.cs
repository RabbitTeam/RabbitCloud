using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions.Filters
{
    public interface IAsyncRequestFilter : IFilterMetadata
    {
        Task OnRequestExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next);
    }
}