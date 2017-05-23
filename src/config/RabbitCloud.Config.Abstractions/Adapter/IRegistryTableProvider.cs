using RabbitCloud.Registry.Abstractions;

namespace RabbitCloud.Config.Abstractions.Adapter
{
    public interface IRegistryTableProvider
    {
        string Name { get; }

        IRegistryTable CreateRegistryTable(RegistryConfig config);
    }
}