using RabbitCloud.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    /// <summary>
    /// 通知监听委托。
    /// </summary>
    /// <param name="registryUrl">注册中心Url。</param>
    /// <param name="urls">发生变更的Url集合。</param>
    /// <returns>一个任务。</returns>
    public delegate Task NotifyListenerDelegate(Url registryUrl, Url[] urls);

    /// <summary>
    /// 一个抽象的发现服务。
    /// </summary>
    public interface IDiscoveryService
    {
        /// <summary>
        /// 订阅一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        Task Subscribe(Url url, NotifyListenerDelegate listener);

        /// <summary>
        /// 取消订阅一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        Task UnSubscribe(Url url, NotifyListenerDelegate listener);

        /// <summary>
        /// 发现注册中心中的所有服务。
        /// </summary>
        /// <param name="url">注册执行Url。</param>
        /// <returns>服务集合。</returns>
        Task<Url[]> Discover(Url url);
    }
}