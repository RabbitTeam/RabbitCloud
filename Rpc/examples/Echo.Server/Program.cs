using Rabbit.Rpc.Address;
using Rabbit.Rpc.Convertibles;
using Rabbit.Rpc.Convertibles.Implementation;
using Rabbit.Rpc.Ids;
using Rabbit.Rpc.Ids.Implementation;
using Rabbit.Rpc.Logging;
using Rabbit.Rpc.Routing;
using Rabbit.Rpc.Routing.Implementation;
using Rabbit.Rpc.Serialization;
using Rabbit.Rpc.Serialization.Implementation;
using Rabbit.Rpc.Server;
using Rabbit.Rpc.Server.Implementation;
using Rabbit.Rpc.Server.Implementation.ServiceDiscovery;
using Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Attributes;
using Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Implementation;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Echo.Server
{
    internal class Program
    {
        static Program()
        {
            //因为没有引用Echo.Common中的任何类型
            //所以强制加载Echo.Common程序集以保证Echo.Common在AppDomain中被加载。
            Assembly.Load("Echo.Common");
        }

        private static void Main()
        {
            //相关服务初始化。
            ISerializer<string> serializer = new JsonSerializer();
            ISerializer<byte[]> byteArraySerializer = new StringByteArraySerializer(serializer);
            ISerializer<object> objectSerializer = new StringObjectSerializer(serializer);
            IServiceIdGenerator serviceIdGenerator = new DefaultServiceIdGenerator(new ConsoleLogger<DefaultServiceIdGenerator>());
            IServiceInstanceFactory serviceInstanceFactory = new DefaultServiceInstanceFactory(new ConsoleLogger<DefaultServiceInstanceFactory>());
            ITypeConvertibleService typeConvertibleService = new DefaultTypeConvertibleService(new[] { new DefaultTypeConvertibleProvider(objectSerializer) }, new NullLogger<DefaultTypeConvertibleService>());
            IClrServiceEntryFactory clrServiceEntryFactory = new ClrServiceEntryFactory(serviceInstanceFactory, serviceIdGenerator, typeConvertibleService);
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetExportedTypes());
            var serviceEntryProvider = new AttributeServiceEntryProvider(types, clrServiceEntryFactory, new ConsoleLogger<AttributeServiceEntryProvider>());
            IServiceEntryManager serviceEntryManager = new DefaultServiceEntryManager(new IServiceEntryProvider[] { serviceEntryProvider });
            IServiceEntryLocate serviceEntryLocate = new DefaultServiceEntryLocate(serviceEntryManager);

            //自动生成服务路由（这边的文件与Echo.Client为强制约束）
            {
                var addressDescriptors = serviceEntryManager.GetEntries().Select(i => new ServiceRoute
                {
                    Address = new[] { new IpAddressModel { Ip = "127.0.0.1", Port = 9981 } },
                    ServiceDescriptor = i.Descriptor
                });

                var serviceRouteManager = new SharedFileServiceRouteManager("d:\\routes.txt", serializer, new ConsoleLogger<SharedFileServiceRouteManager>());
                //zookeeper服务路由管理者。
                //                var serviceRouteManager = new ZooKeeperServiceRouteManager(new ZooKeeperServiceRouteManager.ZookeeperConfigInfo("172.18.20.132:2181"), serializer, new ConsoleLogger<ZooKeeperServiceRouteManager>());
                serviceRouteManager.AddRoutesAsync(addressDescriptors).Wait();
            }

            IServiceHost serviceHost = new NettyServiceHost(new DefaultServiceExecutor(serviceEntryLocate, byteArraySerializer, new ConsoleLogger<DefaultServiceExecutor>()), new ConsoleLogger<NettyServiceHost>(), byteArraySerializer);

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