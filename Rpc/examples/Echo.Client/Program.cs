using Echo.Common;
using Rabbit.Rpc.Client.Address.Resolvers;
using Rabbit.Rpc.Client.Address.Resolvers.Implementation;
using Rabbit.Rpc.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Rabbit.Rpc.Client.Implementation;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Exceptions;
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
            ISerializer<string> serializer = new JsonSerializer();
            ISerializer<byte[]> byteArraySerializer = new StringByteArraySerializer(serializer);
            ISerializer<object> objectSerializer = new StringObjectSerializer(serializer);
            IServiceIdGenerator serviceIdGenerator = new DefaultServiceIdGenerator(new ConsoleLogger<DefaultServiceIdGenerator>());

            var typeConvertibleService = new DefaultTypeConvertibleService(new[] { new DefaultTypeConvertibleProvider(objectSerializer) }, new ConsoleLogger<DefaultTypeConvertibleService>());
            var serviceRouteManager = new SharedFileServiceRouteManager("d:\\routes.txt", serializer, new ConsoleLogger<SharedFileServiceRouteManager>());
            //zookeeper服务路由管理者。
            //            var serviceRouteManager = new ZooKeeperServiceRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181"), serializer, new ConsoleLogger<ZooKeeperServiceRouteManager>());
            //            IAddressSelector addressSelector = new RandomAddressSelector();
            IAddressSelector addressSelector = new PollingAddressSelector();
            IAddressResolver addressResolver = new DefaultAddressResolver(serviceRouteManager, new ConsoleLogger<DefaultAddressResolver>(), addressSelector);
            ITransportClientFactory transportClientFactory = new TransportClientFactory(byteArraySerializer, new ConsoleLogger<TransportClientFactory>());
            var remoteInvokeService = new RemoteInvokeService(addressResolver, transportClientFactory, new ConsoleLogger<RemoteInvokeService>());

            //服务代理相关。
            IServiceProxyGenerater serviceProxyGenerater = new ServiceProxyGenerater(serviceIdGenerator);
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();
            IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(remoteInvokeService, typeConvertibleService);

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).IsAssignableFrom));

            var logger = new ConsoleLogger();
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
                        logger.Error(remoteException.Message);
                    }
                }).Wait();
                Console.ReadLine();
            }
        }
    }
}