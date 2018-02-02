using Rabbit.Go.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Go.Core
{
    public interface IGoClient
    {
        Task RequestAsync(GoContext context);
    }
}