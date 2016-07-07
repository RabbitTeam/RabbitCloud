using Echo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Coordinate.Zookeeper;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Transport.Simple;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Performances.NetCoreApp.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            while (true)
            {
                var serviceCollection = new ServiceCollection();

                var builder = serviceCollection
                    .AddLogging()
                    .AddClient()
                    .UseZooKeeperRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181"))
                    .UseSimpleTransport();

                IServiceProvider serviceProvider = null;
                while (serviceProvider == null)
                {
                    Console.WriteLine("请输入编解码器协议：");
                    Console.WriteLine("1.JSON");
                    Console.WriteLine("2.ProtoBuffer");
                    var codec = Console.ReadLine();
                    switch (codec)
                    {
                        case "1":
                            serviceProvider = serviceCollection.BuildServiceProvider();
                            break;

                        case "2":
                            builder.UseProtoBufferCodec();
                            serviceProvider = serviceCollection.BuildServiceProvider();
                            break;

                        default:
                            Console.WriteLine("输入错误。");
                            continue;
                    }
                }

                serviceProvider.GetRequiredService<ILoggerFactory>()
                    .AddConsole((c, l) => (int)l >= 3);

                var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
                var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
                var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService) }).ToArray();

                //创建IUserService的代理。
                var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom));

                Task.Run(async () =>
                {
                    //预热
                    await userService.GetUser(1);

                    do
                    {
                        Console.WriteLine("正在循环 1w次调用 GetUser.....");
                        //1w次调用
                        var watch = Stopwatch.StartNew();
                        for (var i = 0; i < 10000; i++)
                        {
                            await userService.GetUser(1);
                        }
                        watch.Stop();
                        Console.WriteLine($"1w次调用结束，执行时间：{watch.ElapsedMilliseconds}ms");
                        Console.ReadLine();
                    } while (true);
                }).Wait();
            }
        }
    }
}