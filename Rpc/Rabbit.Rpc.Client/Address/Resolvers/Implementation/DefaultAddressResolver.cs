using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Address.Resolvers.Implementation
{
    /// <summary>
    /// 一个人默认的服务地址解析器。
    /// </summary>
    public class DefaultAddressResolver : IAddressResolver
    {
        #region Field

        private readonly IServiceRouteManager _serviceRoutingService;

        #endregion Field

        #region Constructor

        public DefaultAddressResolver(IServiceRouteManager serviceRoutingService)
        {
            _serviceRoutingService = serviceRoutingService;
        }

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        public async Task<AddressModel> Resolver(string serviceId)
        {
            var descriptors = await _serviceRoutingService.GetRoutesAsync();
            var descriptor = descriptors.FirstOrDefault(i => i.ServiceDescriptor.Id == serviceId);
            return descriptor?.Address.FirstOrDefault();
        }

        #endregion Implementation of IAddressResolver
    }
}