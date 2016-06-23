using Echo.Common;
using Rabbit.Rpc.Client.Address.Resolvers;
using Rabbit.Rpc.Client.Address.Resolvers.Implementation;
using Rabbit.Rpc.Client.Implementation;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Routing.Implementation;
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
            //客户端基本服务。
            ISerializer serializer = new JsonSerializer();
            IServiceIdGenerator serviceIdGenerator = new DefaultServiceIdGenerator(new ConsoleLogger<DefaultServiceIdGenerator>());

            var typeConvertibleService = new DefaultTypeConvertibleService(new[] { new DefaultTypeConvertibleProvider(serializer) }, new ConsoleLogger<DefaultTypeConvertibleService>());
            var serviceRouteManager = new SharedFileServiceRouteManager("d:\\routes.txt", serializer, new ConsoleLogger<SharedFileServiceRouteManager>());
            //zookeeper服务路由管理者。
            //            var serviceRouteManager = new ZooKeeperServiceRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181"), serializer, new ConsoleLogger<ZooKeeperServiceRouteManager>());
            IAddressResolver addressResolver = new DefaultAddressResolver(serviceRouteManager, new ConsoleLogger<DefaultAddressResolver>());
            ITransportClientFactory transportClientFactory = new NettyTransportClientFactory(serializer, new ConsoleLogger<NettyTransportClientFactory>());
            var remoteInvokeService = new RemoteInvokeService(addressResolver, transportClientFactory, new ConsoleLogger<RemoteInvokeService>());

            //服务代理相关。
            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(serviceIdGenerator);
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();
            IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(remoteInvokeService, serializer, typeConvertibleService);

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).IsAssignableFrom));

            while (true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine(DateTime.Now);
                        Console.WriteLine($"userService.GetUserName:{await userService.GetUserName(1)}");
                        Console.WriteLine($"userService.GetUserId:{await userService.GetUserId("rabbit")}");
                        Console.WriteLine($"userService.GetUserLastSignInTime:{await userService.GetUserLastSignInTime(1)}");
                        Console.WriteLine($"userService.Exists:{await userService.Exists(1)}");
                        var user = await userService.GetUser(1);
                        Console.WriteLine($"userService.GetUser:name={user.Name},age={user.Age}");
                        Console.WriteLine($"userService.Update:{await userService.Update(1, user)}");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("发生了错误。" + exception.Message);
                    }
                }).Wait();
                Console.ReadLine();
            }
        }
    }
}