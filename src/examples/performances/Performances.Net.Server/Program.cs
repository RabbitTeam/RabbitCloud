using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Coordinate.Zookeeper;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Transport.DotNetty;
using Rabbit.Transport.Simple;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Performances.Net.Server
{
    public class Program
    {
        static Program()
        {
            //因为没有引用Echo.Common中的任何类型
            //所以强制加载Echo.Common程序集以保证Echo.Common在AppDomain中被加载。
            Assembly.Load(new AssemblyName("Echo.Common"));
        }

        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            var builder = serviceCollection
                .AddLogging()
                .AddRpcCore()
                .AddServiceRuntime()
                .UseZooKeeperRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181"));

            IServiceProvider serviceProvider = null;
            do
            {
                Console.WriteLine("请选择测试组合：");
                Console.WriteLine("1.JSON协议+DotNetty传输");
                Console.WriteLine("2.ProtoBuffer协议+DotNetty传输");
                Console.WriteLine("3.JSON协议+Simple传输");
                Console.WriteLine("4.ProtoBuffer协议+Simple传输");
                var codec = Console.ReadLine();
                switch (codec)
                {
                    case "1":
                        builder.UseDotNettyTransport();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    case "2":
                        builder.UseDotNettyTransport().UseProtoBufferCodec();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    case "3":
                        builder.UseSimpleTransport();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    case "4":
                        builder.UseSimpleTransport().UseProtoBufferCodec();
                        serviceProvider = serviceCollection.BuildServiceProvider();
                        break;

                    default:
                        Console.WriteLine("输入错误。");
                        continue;
                }
            } while (serviceProvider == null);

            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole((c, l) => (int)l >= 3);

            //自动生成服务路由（这边的文件与Echo.Client为强制约束）
            {
                var serviceEntryManager = serviceProvider.GetRequiredService<IServiceEntryManager>();
                var addressDescriptors = serviceEntryManager.GetEntries().Select(i => new ServiceRoute
                {
                    Address = new[] { new IpAddressModel { Ip = "127.0.0.1", Port = 9981 } },
                    ServiceDescriptor = i.Descriptor
                });

                var serviceRouteManager = serviceProvider.GetRequiredService<IServiceRouteManager>();
                serviceRouteManager.SetRoutesAsync(addressDescriptors).Wait();
            }

            var serviceHost = serviceProvider.GetRequiredService<IServiceHost>();

            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9981));
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }).Wait();

            Console.WriteLine("按任意键结束本次测试。");
            Console.ReadLine();
        }
    }
}