using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions.Internal
{
    public interface IRegistryLocator
    {
        Task<IRegistry> GetRegistry(Url url);
    }
}