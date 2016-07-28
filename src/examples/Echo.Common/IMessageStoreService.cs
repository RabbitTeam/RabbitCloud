using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace WeChatDistribution.RpcAbstractions
{
    [RpcService]
    public interface IMessageStoreService
    {
        Task StoreAsync(WeChatMessageModel message);
    }
}