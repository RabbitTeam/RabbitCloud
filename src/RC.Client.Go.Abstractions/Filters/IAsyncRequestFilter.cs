using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public interface IAsyncRequestFilter : IFilterMetadata
    {
        Task OnRequestExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next);
    }
}