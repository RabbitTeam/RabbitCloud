using RabbitCloud.Registry.Abstractions;

namespace RabbitCloud.Config.Abstractions.Support
{
    public interface IRegistryTableFactory
    {
        IRegistryTable GetRegistryTable(RegistryConfig config);
    }
}