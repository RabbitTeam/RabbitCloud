using Microsoft.Extensions.Configuration;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Routing.Implementation
{
    /// <summary>
    /// 一个默认的服务路由提供程序。
    /// </summary>
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        #region Field

        private readonly IConfiguration _configuration;

        #endregion Field

        #region Constructor

        public DefaultServiceRouteProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion Constructor

        #region Implementation of IAddressDescriptorProvider

        public class IpAddressDescriptor
        {
            public List<IpAddressModel> Address { get; set; }
            public ServiceDescriptor ServiceDescriptor { get; set; }
        }

        public Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            var list = new List<IpAddressDescriptor>();
            _configuration.Bind(list);
            return Task.FromResult(list.Select(i => new ServiceRoute
            {
                Address = i.Address,
                ServiceDescriptor = i.ServiceDescriptor
            }));
        }

        #endregion Implementation of IAddressDescriptorProvider
    }
}