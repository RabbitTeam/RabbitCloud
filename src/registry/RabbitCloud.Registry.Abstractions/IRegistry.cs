namespace RabbitCloud.Registry.Abstractions
{
    /// <summary>
    /// 一个抽象的注册中心。
    /// </summary>
    public interface IRegistry : IRegistryService, IDiscoveryService
    {
    }
}