using Echo.Common;
using Microsoft.Extensions.Configuration;
using Rabbit.Rpc.Client.Address.Resolvers;
using Rabbit.Rpc.Client.Address.Resolvers.Implementation;
using Rabbit.Rpc.Client.Implementation;
using Rabbit.Rpc.Client.Routing.Implementation;
using Rabbit.Rpc.Convertibles.Implementation;
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
            //服务路由配置信息获取处（与Echo.Server为强制约束）。
            var configuration = new ConfigurationBuilder()
                .SetBasePath("d:\\")
                .AddJsonFile("routes.txt", false, true)
                .Build();

            //客户端基本服务。
            ISerializer serializer = new JsonSerializer();
            IServiceIdGenerator serviceIdGenerator = new DefaultServiceIdGenerator();
            var serviceRouteProvider = new DefaultServiceRouteProvider(configuration.GetSection("routes"));
            var typeConvertibleService = new DefaultTypeConvertibleService(new[] { new DefaultTypeConvertibleProvider(serializer) });
            var serviceRouteManager = new DefaultServiceRouteManager(new[] { serviceRouteProvider });
            IAddressResolver addressResolver = new DefaultAddressResolver(serviceRouteManager);
            ITransportClientFactory transportClientFactory = new NettyTransportClientFactory(serializer);
            var remoteInvokeService = new RemoteInvokeService(addressResolver, transportClientFactory, serializer);

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