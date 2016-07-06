using Rabbit.Rpc.Address;
using System.Collections.Generic;

namespace Rabbit.Rpc.Routing
{
    /// <summary>
    /// 服务路由。
    /// </summary>
    public class ServiceRoute
    {
        /// <summary>
        /// 服务可用地址。
        /// </summary>
        public IEnumerable<AddressModel> Address { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}