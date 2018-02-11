using System.Threading.Tasks;

namespace Rabbit.Go.Interceptors
{
    public interface IAsyncRequestInterceptor : IInterceptorMetadata
    {
        Task OnRequestExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next);
    }
}