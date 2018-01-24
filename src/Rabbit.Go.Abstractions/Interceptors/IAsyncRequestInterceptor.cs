using System.Threading.Tasks;

namespace Rabbit.Go.Interceptors
{
    public interface IAsyncRequestInterceptor : IInterceptorMetadata
    {
        Task OnActionExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next);
    }
}