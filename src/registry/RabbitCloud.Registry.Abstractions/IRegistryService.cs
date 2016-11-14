using RabbitCloud.Abstractions;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions
{
    /// <summary>
    /// 一个抽象的注册服务。
    /// </summary>
    public interface IRegistryService
    {
        /// <summary>
        /// 注册一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>一个任务。</returns>
        Task Register(Url url);

        /// <summary>
        /// 取消注册一个服务。
        /// </summary>
        /// <param name="url">服务Url。</param>
        /// <returns>一个任务。</returns>
        Task UnRegister(Url url);

        /// <summary>
        /// 获取已经注册的所有服务Url。
        /// </summary>
        /// <returns>服务Url集合。</returns>
        Task<Url[]> GetRegisteredServiceUrls();
    }
}