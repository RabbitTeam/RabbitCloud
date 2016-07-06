using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Address;
using Rabbit.Rpc.Codec.ProtoBuffer;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Runtime.Server;
using Rabbit.Transport.Simple;
using System;
using System.Linq;
using System.Net;
using System.Reflection;

#if !NET451
using System.Text;
#endif

using System.Threading.Tasks;

namespace Echo.Server
{
    public class Program
    {
        static Program()
        {
            //因为没有引用Echo.Common中的任何类型
            //所以强制加载Echo.Common程序集以保证Echo.Common在AppDomain中被加载。
#if NET451
            Assembly.Load("Echo.Common");
#else
            Assembly.Load(new AssemblyName("Echo.Common"));
#endif
        }

        public static void Main(string[] args)
        {
#if !NET451
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddRpcCore()
                .UseProtoBufferCodec()
                .AddServiceRuntime()
                .UseSharedFileRouteManager("d:\\routes.txt")
                .UseSimpleTransport();

            var serviceProvider = serviceCollection.BuildServiceProvider();

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
                serviceRouteManager.AddRoutesAsync(addressDescriptors).Wait();
            }

            var serviceHost = serviceProvider.GetRequiredService<IServiceHost>();

            Task.Factory.StartNew(async () =>
            {
                //启动主机
                await serviceHost.StartAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9981));
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            });
            Console.ReadLine();
        }
    }
}