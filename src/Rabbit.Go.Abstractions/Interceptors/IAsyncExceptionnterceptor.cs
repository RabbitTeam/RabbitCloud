using System.Threading.Tasks;

namespace Rabbit.Go.Interceptors
{
    public interface IAsyncExceptionInterceptor : IInterceptorMetadata
    {
        Task OnExceptionAsync(ExceptionInterceptorContext context);
    }
}