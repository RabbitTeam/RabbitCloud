using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    /// <summary>
    /// 轮询的地址选择器。
    /// </summary>
    public class PollingAddressSelector : AddressSelectorBase
    {
        private readonly ConcurrentDictionary<string, Lazy<AddressEntry>> _concurrent =
            new ConcurrentDictionary<string, Lazy<AddressEntry>>();

        public PollingAddressSelector(IServiceRouteManager serviceRouteManager)
        {
            //路由发生变更时重建地址条目。
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
        }

        #region Overrides of AddressSelectorBase

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected override Task<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var key = GetCacheKey(context.Descriptor);
            //根据服务id缓存服务地址。
            var address = _concurrent.GetOrAdd(key, k => new Lazy<AddressEntry>(() => new AddressEntry(context.Address))).Value;

            return Task.FromResult(address.GetAddress());
        }

        #endregion Overrides of AddressSelectorBase

        #region Private Method

        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            Lazy<AddressEntry> value;
            _concurrent.TryRemove(key, out value);
        }

        #endregion Private Method

        #region Help Class

        protected class AddressEntry
        {
            #region Field

            private int _index = -1;
            private readonly int _maxIndex;
            private readonly AddressModel[] _address;

            #endregion Field

            #region Constructor

            public AddressEntry(IEnumerable<AddressModel> address)
            {
                _address = address.ToArray();
                _maxIndex = _address.Length - 1;
            }

            #endregion Constructor

            #region Public Method

            public AddressModel GetAddress()
            {
                //设置为下一个
                if (_maxIndex > _index)
                    _index++;
                else
                    _index = 0;

                return _address[_index];
            }

            #endregion Public Method
        }

        #endregion Help Class
    }
}