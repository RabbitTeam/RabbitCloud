using Rabbit.Rpc.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Routing
{
    /// <summary>
    /// 一个抽象的服务路由管理者。
    /// </summary>
    public interface IServiceRouteManager
    {
        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> GetRoutesAsync();
    }
}