using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Cluster
{
    public interface IHaStrategy
    {
        Task<IResponse> CallAsync(IRequest request, ILoadBalance loadBalance);
    }
}