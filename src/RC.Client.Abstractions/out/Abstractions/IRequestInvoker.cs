using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRequestInvoker
    {
        Task InvokeAsync();
    }
}