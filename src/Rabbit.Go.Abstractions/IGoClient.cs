using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions
{
    public interface IGoClient
    {
        Task RequestAsync(RequestContext context);
    }
}