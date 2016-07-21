using Microsoft.Extensions.DependencyInjection;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using System;
using System.IO;

namespace Rabbit.Rpc.Tests.ServiceRouteManagers
{
    public class SharedFileServiceRouteManagerTests : ServiceRouteManagerTests
    {
        public SharedFileServiceRouteManagerTests()
        {
            var routeFile = Path.Combine(AppContext.BaseDirectory, "routes.txt");
            var services = new ServiceCollection();
            services
                .AddLogging()
                .AddRpcCore()
                .UseSharedFileRouteManager(routeFile);
            var provider = services.BuildServiceProvider();

            ServiceRouteManager = (SharedFileServiceRouteManager)provider.GetRequiredService<IServiceRouteManager>();
        }

        #region Overrides of ServiceRouteManagerTests

        protected override IServiceRouteManager ServiceRouteManager { get; }

        #endregion Overrides of ServiceRouteManagerTests
    }
}