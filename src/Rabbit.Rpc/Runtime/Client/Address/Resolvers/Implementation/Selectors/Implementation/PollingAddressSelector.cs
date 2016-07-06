using Rabbit.Rpc.Address;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation
{
    /// <summary>
    /// 轮询的地址选择器。
    /// </summary>
    public class PollingAddressSelector : AddressSelectorBase
    {
        private readonly ConcurrentDictionary<string, AddressEntry> _concurrent = new ConcurrentDictionary<string, AddressEntry>();

        #region Overrides of AddressSelectorBase

        /// <summary>
        /// 选择一个地址。
        /// </summary>
        /// <param name="context">地址选择上下文。</param>
        /// <returns>地址模型。</returns>
        protected override Task<AddressModel> SelectAsync(AddressSelectContext context)
        {
            var key = context.ServiceRoute.ServiceDescriptor.Id;
            //根据服务id缓存服务地址。
            var address = _concurrent.AddOrUpdate(key, k => new AddressEntry(context.ServiceRoute.Address), (k, currentEntry) =>
              {
                  var newAddress = context.ServiceRoute.Address.ToArray();
                  var currentAddress = currentEntry.Address;
                  var unionAddress = currentEntry.Address.Union(newAddress).ToArray();

                  if (unionAddress.Length != currentAddress.Length)
                      return new AddressEntry(newAddress);

                  if (unionAddress.Any(addressModel => !newAddress.Contains(addressModel)))
                  {
                      return new AddressEntry(newAddress);
                  }

                  return currentEntry;
              });

            return Task.FromResult(address.GetAddress());
        }

        #endregion Overrides of AddressSelectorBase

        #region Help Class

        protected class AddressEntry
        {
            #region Field

            private int _index;
            private int _lock;
            private readonly int _maxIndex;

            #endregion Field

            #region Constructor

            public AddressEntry(IEnumerable<AddressModel> address)
            {
                Address = address.ToArray();
                _maxIndex = Address.Length - 1;
            }

            #endregion Constructor

            #region Property

            public AddressModel[] Address { get; set; }

            #endregion Property

            #region Public Method

            public AddressModel GetAddress()
            {
                while (true)
                {
                    //如果无法得到锁则等待
                    if (Interlocked.Exchange(ref _lock, 1) != 0)
                        continue;

                    var address = Address[_index];

                    //设置为下一个
                    if (_maxIndex > _index)
                        _index++;
                    else
                        _index = 0;

                    //释放锁
                    Interlocked.Exchange(ref _lock, 0);

                    return address;
                }
            }

            #endregion Public Method
        }

        #endregion Help Class
    }
}