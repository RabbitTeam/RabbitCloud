using RabbitCloud.Rpc.Abstractions.Protocol;
using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions.Internal
{
    public interface IProtocolLocator
    {
        Task<IProtocol> GetProtocol(string name);
    }
}