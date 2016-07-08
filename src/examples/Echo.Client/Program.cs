using Echo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Transport.Simple;
using System;
using System.Linq;
using System.Reflection;

#if !NET
using System.Text;
#endif

using System.Threading.Tasks;

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
                .UseSharedFileRouteManager(System.IO.Path.Combine(AppContext.BaseDirectory,"routes.txt"))
#else
                .UseSharedFileRouteManager("d:\\routes.txt")
#endif
                .UseSimpleTransport()
                .UseProtoBufferCodec();

            var serviceProvider = serviceCollection.BuildServiceProvider();

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