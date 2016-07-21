using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabbit.Rpc.Tests.ServiceRouteManagers
{
    public abstract class ServiceRouteManagerTests : IDisposable
    {
        protected abstract IServiceRouteManager ServiceRouteManager { get; }

        [Fact]
        public async Task BasicRouteTest()
        {
            await Clear();

            var routes = (await ServiceRouteManager.GetRoutesAsync()).ToArray();
            Assert.False(routes.Any());

            var newRoute =
                new ServiceRoute
                {
                    Address = new[]
                    {
                        new IpAddressModel("127.0.0.1", 1)
                    },
                    ServiceDescriptor = new ServiceDescriptor { Id = "service1" }
                };

            await ServiceRouteManager.SetRoutesAsync(new[]
            {
                newRoute
            });

            await Task.Delay(2000);

            routes = (await ServiceRouteManager.GetRoutesAsync()).ToArray();

            Assert.Equal(newRoute, routes.First());
            Assert.True(routes.Any());
            Assert.Equal(1, routes.Length);

            await Clear();

            routes = (await ServiceRouteManager.GetRoutesAsync()).ToArray();
            Assert.False(routes.Any());
        }

        [Fact]
        public async Task EventTest()
        {
            var route1 = new ServiceRoute
            {
                Address = new[]
                {
                    new IpAddressModel("127.0.0.1", 1)
                },
                ServiceDescriptor = new ServiceDescriptor
                {
                    Id = "service1"
                }
            };
            var route2 = new ServiceRoute
            {
                Address = new[]
                {
                    new IpAddressModel("127.0.0.1", 2)
                },
                ServiceDescriptor = new ServiceDescriptor
                {
                    Id = "service2"
                }
            };
            var route3 = new ServiceRoute
            {
                Address = new[]
                {
                    new IpAddressModel("127.0.0.1", 3)
                },
                ServiceDescriptor = new ServiceDescriptor
                {
                    Id = "service3"
                }
            };

            await ServiceRouteManager.ClearAsync();
            await Task.Delay(2000);

            var initWait = new TaskCompletionSource<bool>();
            ServiceRouteManager.Created += (s, e) => { initWait.TrySetResult(true); };
            await ServiceRouteManager.SetRoutesAsync(new[] { route1, route2 });
            await initWait.Task;
            await Task.Delay(2000);

            route2.Address = new[]
            {
                new IpAddressModel("127.0.0.1", 11)
            };

            TaskCompletionSource<bool> createdWait = null, changedWait = null, removedWait = null;
            Action reset = () =>
            {
                createdWait = new TaskCompletionSource<bool>();
                changedWait = new TaskCompletionSource<bool>();
                removedWait = new TaskCompletionSource<bool>();
            };
            reset();
            ServiceRouteManager.Created +=
                (s, e) => { createdWait.TrySetResult(route3.ServiceDescriptor.Id == e.Route.ServiceDescriptor.Id); };
            ServiceRouteManager.Changed +=
                (s, e) =>
                {
                    changedWait.TrySetResult(
                        route2.ServiceDescriptor.Id == e.Route.ServiceDescriptor.Id
                        && route2.Address.First() == e.Route.Address.First()
                        && 2 == ((IpAddressModel)e.OldRoute.Address.First()).Port);
                };
            ServiceRouteManager.Removed +=
                (s, e) => { removedWait.TrySetResult(route1.ServiceDescriptor.Id == e.Route.ServiceDescriptor.Id); };

            await ServiceRouteManager.SetRoutesAsync(new[] { route2, route3 });

            Assert.True(await createdWait.Task);
            Assert.True(await changedWait.Task);
            Assert.True(await removedWait.Task);
        }

        private async Task Clear()
        {
            var routes = (await ServiceRouteManager.GetRoutesAsync()).ToArray();

            if (routes.Any())
            {
                var clearWait = new TaskCompletionSource<bool>();
                var removeCount = 0;
                ServiceRouteManager.Removed += (s, e) =>
                {
                    if (clearWait.Task.IsCompleted)
                        return;
                    removeCount++;
                    if (removeCount == routes.Length)
                        clearWait.TrySetResult(true);
                };
                await ServiceRouteManager.ClearAsync();
                await clearWait.Task;
            }
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public async void Dispose()
        {
            await Clear();
        }

        #endregion Implementation of IDisposable
    }
}