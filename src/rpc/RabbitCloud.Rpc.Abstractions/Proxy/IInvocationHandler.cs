using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public interface IInvocationHandler
    {
        Task<object> Invoke(object proxy, MethodInfo method, object[] args);
    }
}