using RabbitCloud.Abstractions;

namespace RabbitCloud.Registry.Abstractions
{
    /// <summary>
    /// 一个抽象的注册中心工厂。
    /// </summary>
    public interface IRegistryFactory
    {
        /// <summary>
        /// 获取一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <returns>注册中心。</returns>
        IRegistry GetRegistry(Url url);
    }
}