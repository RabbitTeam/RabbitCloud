using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Routing
{
    /// <summary>
    /// 一个抽象的服务路由发现者。
    /// </summary>
    public interface IServiceRouteManager
    {
        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> GetRoutesAsync();

        /// <summary>
        /// 添加服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        Task AddRoutesAsync(IEnumerable<ServiceRoute> routes);

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        Task ClearAsync();
    }
}