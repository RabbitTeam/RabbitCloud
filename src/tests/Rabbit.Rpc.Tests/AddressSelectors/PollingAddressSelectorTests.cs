using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Rabbit.Rpc.Serialization.Implementation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rabbit.Rpc.Tests.AddressSelectors
{
    public class PollingAddressSelectorTests
    {
        private class TestServiceRouteManager : ServiceRouteManagerBase
        {
            private ServiceRoute[] _routes;
            private readonly IServiceRouteFactory _serviceRouteFactory = new DefaultServiceRouteFactory(new JsonSerializer());

            public TestServiceRouteManager() : base(new JsonSerializer())
            {
                Reset();
            }

            #region Overrides of ServiceRouteManagerBase

            /// <summary>
            /// 获取所有可用的服务路由信息。
            /// </summary>
            /// <returns>服务路由集合。</returns>
            public override Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
            {
                return Task.FromResult(_routes.AsEnumerable());
            }

            /// <summary>
            /// 清空所有的服务路由。
            /// </summary>
            /// <returns>一个任务。</returns>
            public override Task ClearAsync()
            {
                OnRemoved(new ServiceRouteEventArgs(_routes[0]));
                _routes = new ServiceRoute[0];
                return Task.CompletedTask;
            }

            /// <summary>
            /// 设置服务路由。
            /// </summary>
            /// <param name="routes">服务路由集合。</param>
            /// <returns>一个任务。</returns>
            protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
            {
                var newRoutes = (await _serviceRouteFactory.CreateServiceRoutesAsync(routes)).ToArray();
                OnChanged(new ServiceRouteChangedEventArgs(newRoutes.First(), _routes[0]));
                _routes = newRoutes;
            }

            #endregion Overrides of ServiceRouteManagerBase

            public void Reset()
            {
                _routes = new[]
                {
                    new ServiceRoute
                    {
                        Address = Enumerable.Range(1, 100).Select(i => new IpAddressModel("127.0.0.1", i)),
                        ServiceDescriptor = new ServiceDescriptor
                        {
                            Id = "service1"
                        }
                    }
                };
            }
        }

        private readonly IServiceRouteManager _serviceRouteManager = new TestServiceRouteManager();

        private AddressSelectContext GetSelectContext()
        {
            var route = _serviceRouteManager.GetRoutesAsync().Result.First();
            return new AddressSelectContext
            {
                Address = route.Address,
                Descriptor = route.ServiceDescriptor
            };
        }

        [Fact]
        public async void PollingAddressSyncTest()
        {
            IAddressSelector selector = new PollingAddressSelector(_serviceRouteManager);

            var context = GetSelectContext();

            var numbers = new List<int>();
            for (var i = 0; i < 500; i++)
            {
                var address = (IpAddressModel)await selector.SelectAsync(context);
                numbers.Add(address.Port);
            }

            var isOk = true;
            for (var i = 0; i < numbers.Count; i++)
            {
                if (numbers.Count == i + 1)
                    break;
                var current = numbers[i];
                var next = numbers[i + 1];
                if (current == next - 1)
                    continue;
                isOk = false;
                break;
            }
            if (isOk)
                Assert.True(true);
            else
            {
                Assert.False(false, string.Join("", numbers));
            }
        }

        protected class AddressEntry
        {
            private readonly IList<int> _indexs;

            #region Field

            private int _index;
            private int _lock;
            private readonly int _maxIndex;
            private readonly AddressModel[] _address;

            #endregion Field

            #region Constructor

            public AddressEntry(IEnumerable<AddressModel> address, IList<int> indexs)
            {
                _indexs = indexs;
                _address = address.ToArray();
                _maxIndex = _address.Length - 1;
            }

            #endregion Constructor

            #region Public Method

            public AddressModel GetAddress()
            {
                while (true)
                {
                    //如果无法得到锁则等待
                    if (Interlocked.Exchange(ref _lock, 1) != 0)
                    {
                        default(SpinWait).SpinOnce();
                        continue;
                    }

                    _indexs.Add(_index);

                    var address = _address[_index];

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

        [Fact]
        public void PollingAddressAsyncTest()
        {
            var context = GetSelectContext();
            var indexs = new List<int>();
            var entry = new AddressEntry(context.Address, indexs);

            var status = Parallel.For(0, 200, index =>
            {
                entry.GetAddress();
            });
            while (!status.IsCompleted)
            {
                Thread.Sleep(10);
            }
            for (var i = 0; i < indexs.Count; i++)
            {
                if (indexs.Count == i + 1)
                    break;
                var current = indexs.ElementAt(i);
                var next = indexs.ElementAt(i + 1);
                Assert.True((next == 0 && current == 99) || current == next - 1);
            }
        }

        [Fact]
        public async void PollingAddressChangeTest()
        {
            IAddressSelector selector = new PollingAddressSelector(_serviceRouteManager);

            await selector.SelectAsync(GetSelectContext());
            await selector.SelectAsync(GetSelectContext());
            var address = (IpAddressModel)await selector.SelectAsync(GetSelectContext());

            Assert.Equal(3, address.Port);

            //更新路由信息。
            await _serviceRouteManager.SetRoutesAsync(new[]
            {
                new ServiceRoute
                {
                    Address = new[]
                    {
                        new IpAddressModel("127.0.0.1", 0),
                        new IpAddressModel("127.0.0.1", 2)
                    },
                    ServiceDescriptor = new ServiceDescriptor
                    {
                        Id = "service1"
                    }
                }
            });

            address = (IpAddressModel)await selector.SelectAsync(GetSelectContext());
            Assert.Equal(0, address.Port);
            address = (IpAddressModel)await selector.SelectAsync(GetSelectContext());
            Assert.Equal(2, address.Port);

            ((TestServiceRouteManager)_serviceRouteManager).Reset();
        }
    }
}