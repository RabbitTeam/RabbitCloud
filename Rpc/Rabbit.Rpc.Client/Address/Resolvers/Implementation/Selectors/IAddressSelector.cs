using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Client.Address.Resolvers.Implementation.Selectors
{
    /// <summary>
    /// 地址选择上下文。
    /// </summary>
    public class AddressSelectContext
    {
        /// <summary>
        /// 服务路由。
        /// </summary>
        public ServiceRoute ServiceRoute { get; set; }
    }

    /// <summary>
    /// 一个抽象的地址选择器。
    /// </summary>
    public interface IAddressSelector
    {
        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        Task<AddressModel> SelectAsync(AddressSelectContext context);
    }
}