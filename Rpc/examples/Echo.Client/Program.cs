using Echo.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Rabbit.Rpc.Client.Address.Resolvers;
using Rabbit.Rpc.Client.Address.Resolvers.Implementation;
using Rabbit.Rpc.Client.Implementation;
using Rabbit.Rpc.Client.Routing.Implementation;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using Rabbit.Rpc.Transport;
using Rabbit.Rpc.Transport.Implementation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Echo.Client
{
    internal class Program
    {
        private static void Main()
        {
//            new PhysicalFileProvider("d:\\").Watch("routes.txt");
            //服务路由配置信息获取处（与Echo.Server为强制约束）。
            var configuration = new ConfigurationBuilder()
                .SetBasePath("d:\\")
                .AddJsonFile("routes.txt", false, true)
//                .SetFileProvider(new PhysicalFileProvider("d:\\"))
                .Build();

            //客户端基本服务。
            ISerializer serializer = new JsonSerializer();
            IServiceIdGenerator serviceIdGenerator = new DefaultServiceIdGenerator();
            var serviceRouteProvider = new DefaultServiceRouteProvider(configuration.GetSection("routes"));
            var serviceRouteManager = new DefaultServiceRouteManager(new[] { serviceRouteProvider });
            IAddressResolver addressResolver = new DefaultAddressResolver(serviceRouteManager);
            ITransportClientFactory transportClientFactory = new NettyTransportClientFactory(serializer);
            var remoteInvokeService = new RemoteInvokeService(addressResolver, transportClientFactory, serializer);

            //服务代理相关。
            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(serviceIdGenerator);
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();
            IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(remoteInvokeService, serializer);

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).IsAssignableFrom));

            while (true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"userService.GetUserName:{await userService.GetUserName(1)}");
                    }
                    catch
                    {
                        Console.WriteLine("发生了错误。");
                    }
                }).Wait();
                Console.ReadLine();
            }
        }
    }
}