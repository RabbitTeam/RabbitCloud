using RabbitCloud.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IInvoker
    {
        Url Url { get; }

        Task<IResult> Invoke(IInvocation invocation);
    }
}