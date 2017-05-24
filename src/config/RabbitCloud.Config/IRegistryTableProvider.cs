using RabbitCloud.Config.Abstractions;
using RabbitCloud.Registry.Abstractions;

namespace RabbitCloud.Config
{
    public interface IRegistryTableProvider
    {
        string Name { get; }

        IRegistryTable CreateRegistryTable(RegistryConfig config);
    }
}