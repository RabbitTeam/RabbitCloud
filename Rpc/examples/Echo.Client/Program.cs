using Echo.Common;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Rpc.ProxyGenerator.Implementation;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Rabbit.Rpc.Runtime.Client.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using Rabbit.Rpc.Transport;
using Rabbit.Transport.DotNetty;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Echo.Client
{
    internal class Program
    {
        private static void Main()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole((c, l) => (int)l >= 3);

            //客户端基本服务。
            ISerializer<string> serializer = new JsonSerializer();
            ISerializer<byte[]> byteArraySerializer = new StringByteArraySerializer(serializer);
            ISerializer<object> objectSerializer = new StringObjectSerializer(serializer);
            IServiceIdGenerator serviceIdGenerator = new DefaultServiceIdGenerator(loggerFactory.CreateLogger<DefaultServiceIdGenerator>());

            var typeConvertibleService = new DefaultTypeConvertibleService(new[] { new DefaultTypeConvertibleProvider(objectSerializer) }, loggerFactory.CreateLogger<DefaultTypeConvertibleService>());
            var serviceRouteManager = new SharedFileServiceRouteManager("d:\\routes.txt", serializer, loggerFactory.CreateLogger<SharedFileServiceRouteManager>());
            //zookeeper服务路由管理者。
            //            var serviceRouteManager = new ZooKeeperServiceRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181"), serializer, new ConsoleLogger<ZooKeeperServiceRouteManager>());
            //            IAddressSelector addressSelector = new RandomAddressSelector();
            IAddressSelector addressSelector = new PollingAddressSelector();
            IAddressResolver addressResolver = new DefaultAddressResolver(serviceRouteManager, loggerFactory.CreateLogger<DefaultAddressResolver>(), addressSelector);
            ITransportClientFactory transportClientFactory = new DotNettyTransportClientFactory(byteArraySerializer, objectSerializer, loggerFactory.CreateLogger<DotNettyTransportClientFactory>());
            var remoteInvokeService = new RemoteInvokeService(addressResolver, transportClientFactory, loggerFactory.CreateLogger<RemoteInvokeService>());

            //服务代理相关。
            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(serviceIdGenerator);
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();
            IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(remoteInvokeService, typeConvertibleService);

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).IsAssignableFrom));

            var logger = loggerFactory.CreateLogger(typeof(Program));
            while (true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"userService.GetUserName:{await userService.GetUserName(1)}");
                        Console.WriteLine($"userService.GetUserId:{await userService.GetUserId("rabbit")}");
                        Console.WriteLine($"userService.GetUserLastSignInTime:{await userService.GetUserLastSignInTime(1)}");
                        Console.WriteLine($"userService.Exists:{await userService.Exists(1)}");
                        var user = await userService.GetUser(1);
                        Console.WriteLine($"userService.GetUser:name={user.Name},age={user.Age}");
                        Console.WriteLine($"userService.Update:{await userService.Update(1, user)}");
                        Console.WriteLine($"userService.GetDictionary:{(await userService.GetDictionary())["key"]}");
                        await userService.TryThrowException();
                    }
                    catch (RpcRemoteException remoteException)
                    {
                        logger.LogError(remoteException.Message);
                    }
                }).Wait();
                Console.ReadLine();
            }
        }
    }
}