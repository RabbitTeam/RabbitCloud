using RabbitCloud.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IInvoker
    {
        Id Id { get; }

        Task<IResult> Invoke(IInvocation invocation);
    }
}