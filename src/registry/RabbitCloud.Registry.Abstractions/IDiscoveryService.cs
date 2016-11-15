using RabbitCloud.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    /// <summary>
    /// 一个抽象的发现服务。
    /// </summary>
    public interface IDiscoveryService
    {
        /// <summary>
        /// 订阅一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        Task Subscribe(Url url, NotifyListenerDelegate listener);

        /// <summary>
        /// 取消订阅。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <param name="listener">监听器。</param>
        /// <returns>一个任务。</returns>
        Task UnSubscribe(Url url, NotifyListenerDelegate listener);

        /// <summary>
        /// 发现注册中心中指定服务的所有节点。
        /// </summary>
        /// <param name="url">注册的服务Url。</param>
        /// <returns>服务节点集合。</returns>
        Task<Url[]> Discover(Url url);
    }
}