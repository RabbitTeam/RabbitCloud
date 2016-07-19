using Echo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Transport.Simple;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#if !NET

using System.Text;

#endif

using System.Threading.Tasks;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Rabbit.Rpc.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;

namespace Echo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {

#if !NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddClient()
#if !NET
                .UseSharedFileRouteManager(System.IO.Path.Combine(AppContext.BaseDirectory, "routes.txt"))
#else
                .UseSharedFileRouteManager("d:\\routes.txt")
#endif
                .UseSimpleTransport()
                .UseProtoBufferCodec();

            var serviceProvider = serviceCollection.BuildServiceProvider();


            Task.Run(async () =>
            {
                var context = new AddressSelectContext
                {
                    Address = Enumerable.Range(1, 100).Select(i => new IpAddressModel("127.0.0.1", i)),
                    Descriptor = new Rabbit.Rpc.ServiceDescriptor
                    {
                        Id = "service1"
                    }
                };
                IAddressSelector s = new PollingAddressSelector(serviceProvider.GetRequiredService<IServiceRouteManager>());

                for (var i = 0; i < 1000; i++)
                {
                    await s.SelectAsync(context);
                }

                var watch = Stopwatch.StartNew();
                for (var i = 0; i < 1000; i++)
                {
                    await s.SelectAsync(context);
                }
                watch.Stop();
                Console.WriteLine(watch.Elapsed.Milliseconds + "ms");
            }).Wait();
            return;

            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole((c, l) => (int)l >= 3);

            var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
            var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom));

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
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