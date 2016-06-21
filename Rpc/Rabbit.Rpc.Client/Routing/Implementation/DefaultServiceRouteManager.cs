using Rabbit.Rpc.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Routing.Implementation
{
    /// <summary>
    /// 一个默认的服务路由管理者。
    /// </summary>
    public class DefaultServiceRouteManager : IServiceRouteManager
    {
        private readonly IEnumerable<IServiceRouteProvider> _providers;

        public DefaultServiceRouteManager(IEnumerable<IServiceRouteProvider> providers)
        {
            _providers = providers;
        }

        #region Implementation of IServiceRoutingDescriptorService

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public async Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            var list = new List<ServiceRoute>();
            foreach (var provider in _providers)
            {
                list.AddRange(await provider.GetRoutesAsync());
            }
            return list;
        }

        #endregion Implementation of IServiceRoutingDescriptorService
    }
}