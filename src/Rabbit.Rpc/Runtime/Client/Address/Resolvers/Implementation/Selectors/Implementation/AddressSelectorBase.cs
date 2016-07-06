using Rabbit.Rpc.Address;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    /// <summary>
    /// 地址选择器基类。
    /// </summary>
    public abstract class AddressSelectorBase : IAddressSelector
    {
        #region Implementation of IAddressSelector

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        Task<AddressModel> IAddressSelector.SelectAsync(AddressSelectContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.ServiceRoute == null)
                throw new ArgumentNullException(nameof(context.ServiceRoute));
            if (context.ServiceRoute.Address == null)
                throw new ArgumentNullException(nameof(context.ServiceRoute.Address));

            var address = context.ServiceRoute.Address.ToArray();
            if (!address.Any())
                throw new ArgumentException("没有任何地址信息。", nameof(context.ServiceRoute.Address));

            return address.Length == 1 ? Task.FromResult(address[0]) : SelectAsync(context);
        }

        #endregion Implementation of IAddressSelector

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected abstract Task<AddressModel> SelectAsync(AddressSelectContext context);
    }
}