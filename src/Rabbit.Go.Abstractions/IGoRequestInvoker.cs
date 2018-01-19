using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions
{
    public interface IGoRequestInvoker
    {
        Task InvokeAsync();
    }
}