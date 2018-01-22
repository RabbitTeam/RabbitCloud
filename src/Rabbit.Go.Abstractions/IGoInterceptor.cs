using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions
{
    public interface IGoInterceptor
    {
        Task ApplyAsync(RequestContext context);
    }
}