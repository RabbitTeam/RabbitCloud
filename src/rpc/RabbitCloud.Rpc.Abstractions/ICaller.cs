using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface ICaller
    {
        bool IsAvailable { get; }

        Task<IResponse> CallAsync(IRequest request);
    }
}