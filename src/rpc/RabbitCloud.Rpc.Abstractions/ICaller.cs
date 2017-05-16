using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface ICaller
    {
        Task<IResponse> CallAsync(IRequest request);
    }
}