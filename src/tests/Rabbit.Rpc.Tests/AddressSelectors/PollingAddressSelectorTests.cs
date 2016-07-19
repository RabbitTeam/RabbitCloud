using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Rabbit.Rpc.Serialization.Implementation;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void PollingAddressAsyncTest()
        {
            IAddressSelector selector = new PollingAddressSelector(_serviceRouteManager);

            var context = GetSelectContext();

            var numbers = new List<int>();
            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                var task = Task.Run(async () =>
                  {
                      for (var z = 0; z < 200; z++)
                      {
                          var address = (IpAddressModel)await selector.SelectAsync(context);
                          numbers.Add(address.Port);
                      }
                  });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

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