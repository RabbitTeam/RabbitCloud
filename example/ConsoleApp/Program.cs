using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Registry.Consul;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.NetMQ;
using RabbitCloud.Rpc.NetMQ.Internal;
using RabbitCloud.Rpc.Proxy;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public interface IUserService
    {
        string GetName(long id);

        string GetName(long id, string name);

        Task Test();

        Task<string> Test2();

        void Test3();
    }

    public class UserService : IUserService
    {
        public string GetName(long id)
        {
            return "123";
        }

        public string GetName(long id, string name)
        {
            return id + name;
        }

        public Task Test()
        {
            Console.WriteLine("test");
            return Task.CompletedTask;
        }

        public Task<string> Test2()
        {
            return Task.FromResult("asfasd");
        }

        public void Test3()
        {
            Console.WriteLine("test3");
        }
    }

    public interface ITestService
    {
        string Test();
    }

    public class TestService : ITestService
    {
        #region Implementation of ITestService

        public string Test()
        {
            return "test";
        }

        #endregion Implementation of ITestService
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                #region init

                var jsonRequestFormatter = new JsonRequestFormatter();
                var jsonResponseFormatter = new JsonResponseFormatter();

                IRequestIdGenerator requestIdGenerator = new DefaultRequestIdGenerator();

                var proxyFactory = new ProxyFactory(requestIdGenerator);

                var netMqPollerHolder = new NetMqPollerHolder();
                IRouterSocketFactory responseSocketFactory = new RouterSocketFactory(netMqPollerHolder);

                IProtocol protocol = new NetMqProtocol(responseSocketFactory, jsonRequestFormatter, jsonResponseFormatter, netMqPollerHolder);

                var endPoint = new IPEndPoint(IPAddress.Parse("192.168.5.26"), 9999);

                protocol.Export(new ExportContext
                {
                    Caller = new TypeCaller(new UserService()),
                    EndPoint = endPoint,
                    ServiceKey = new ServiceKey("user")
                });
                protocol.Export(new ExportContext
                {
                    Caller = new TypeCaller(new UserService()),
                    EndPoint = new IPEndPoint(IPAddress.Parse("192.168.5.26"), 8888),
                    ServiceKey = new ServiceKey("user")
                });

                var registryTable = new ConsulRegistryTable(new Uri("http://localhost:8500"));

                #endregion init

                var s1 = new ServiceRegistryDescriptor
                {
                    Host = "192.168.5.26",
                    Port = 9999,
                    Protocol = "netmq",
                    ServiceKey = new ServiceKey("user")
                };
                var s2 = new ServiceRegistryDescriptor
                {
                    Host = "192.168.5.26",
                    Port = 8888,
                    Protocol = "netmq",
                    ServiceKey = new ServiceKey("user")
                };

                registryTable.Subscribe(s1, (s, descriptors) =>
                {
                    Console.WriteLine("s1 change");
                    foreach (var descriptor in descriptors)
                    {
                        Console.WriteLine(descriptor.ServiceKey);
                    }
                });
                registryTable.Subscribe(s2, (s, descriptors) =>
                {
                    Console.WriteLine("s2 change");
                    foreach (var descriptor in descriptors)
                    {
                        Console.WriteLine(descriptor.ServiceKey);
                    }
                });

//                await registryTable.Discover(s1);
//                await registryTable.Discover(s2);

                Console.ReadLine();
                await registryTable.UnRegisterAsync(s1);
                Console.WriteLine("UnRegisterAsync s1");
                Console.ReadLine();
                await registryTable.UnRegisterAsync(s2);
                Console.WriteLine("UnRegisterAsync s2");
                Console.ReadLine();
                await registryTable.RegisterAsync(s1);
                Console.WriteLine("RegisterAsync s1");
                Console.ReadLine();
                await registryTable.RegisterAsync(s2);
                Console.WriteLine("RegisterAsync s2");
                Console.ReadLine();
                
                await registryTable.SetUnAvailableAsync(s1);
                Console.ReadLine();

                await Task.CompletedTask;
            }).Wait();
            Console.ReadLine();
        }
    }
}