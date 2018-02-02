using System.Threading.Tasks;

namespace Rabbit.Go
{
    public interface IGoClient
    {
        Task RequestAsync(GoContext context);
    }
}