using Rabbit.Rpc.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Routing
{
    /// <summary>
    /// 一个抽象的服务路由提供程序。
    /// </summary>
    public interface IServiceRouteProvider
    {
        /// <summary>
        /// 获取服务路由集合。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        Task<IEnumerable<ServiceRoute>> GetRoutesAsync();
    }
}
