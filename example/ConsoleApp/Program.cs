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
                /*using (var client = new ConsulClient())
                {
                    var c = await client.Catalog.Services();
                    foreach (var service in (await client.Agent.Services()).Response)
                    {
                        Console.WriteLine(service.Key);
                        Console.WriteLine(service.Value.Address);
                    }
                }*/

                var jsonRequestFormatter = new JsonRequestFormatter();
                var jsonResponseFormatter = new JsonResponseFormatter();

                IRequestIdGenerator requestIdGenerator = new DefaultRequestIdGenerator();

                var proxyFactory = new ProxyFactory(requestIdGenerator);

                var netMqPollerHolder = new NetMqPollerHolder();
                IRouterSocketFactory responseSocketFactory = new RouterSocketFactory(netMqPollerHolder);

                IProtocol protocol = new NetMqProtocol(responseSocketFactory, jsonRequestFormatter, jsonResponseFormatter, netMqPollerHolder);

                var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);

                protocol.Export(new ExportContext
                {
                    Caller = new TypeCaller(new UserService()),
                    EndPoint = endPoint,
                    ServiceKey = new ServiceKey("user")
                });
                protocol.Export(new ExportContext
                {
                    Caller = new TypeCaller(new UserService()),
                    EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888),
                    ServiceKey = new ServiceKey("user")
                });

                IRegistryTable registryTable = new ConsulRegistryTable(new Uri("http://localhost:8500"));

                var s1 = new ServiceRegistryDescriptor
                {
                    Host = "127.0.0.1",
                    Port = 9999,
                    Protocol = "netmq",
                    ServiceKey = new ServiceKey("user")
                };
                var s2 = new ServiceRegistryDescriptor
                {
                    Host = "127.0.0.1",
                    Port = 8888,
                    Protocol = "netmq",
                    ServiceKey = new ServiceKey("user")
                };
                /*await registryTable.UnRegisterAsync(s1);
                await registryTable.UnRegisterAsync(s2);
                await registryTable.RegisterAsync(s1);
                await registryTable.RegisterAsync(s2);*/

                var result = await registryTable.Discover(s1);
                var i = 0;

                /*

                                var caller = protocol.Refer(new ReferContext
                                {
                                    EndPoint = endPoint,
                                    ServiceKey = new ServiceKey("user")
                                });

                                ICluster cluster = new DefaultCluster
                                {
                                    HaStrategy = new FailoverHaStrategy(new[]
                                    {
                                        caller,
                                        protocol.Refer(new ReferContext
                                        {
                                            EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888),
                                            ServiceKey = new ServiceKey("user")
                                        })
                                    }),
                                    LoadBalance = new RoundRobinLoadBalance()
                                };
                                var userService = proxyFactory.GetProxy<IUserService>(cluster);

                                Console.WriteLine(userService.GetName(1));
                                Console.WriteLine(userService.GetName(1));
                                Console.WriteLine(userService.GetName(1));*/
                Console.ReadLine();
                return;
                /*                var watch = new Stopwatch();
                                watch.Start();
                                for (var i = 0; i < 10000; i++)
                                {
                                    userService.GetName(1);
                                }
                                watch.Stop();
                                Console.WriteLine(watch.ElapsedMilliseconds + "ms");*/

                /*                protocol.Export(new ExportContext
                                {
                                    Caller = new TypeCaller(new TestService()),
                                    EndPoint = endPoint,
                                    ServiceKey = new ServiceKey("ITestService")
                                });

                                var callerTest = protocol.Refer(new ReferContext
                                {
                                    EndPoint = endPoint,
                                    ServiceKey = new ServiceKey("ITestService")
                                });

                                var testService = proxyFactory.GetProxy<ITestService>(callerTest);
                                Console.WriteLine(testService.Test());*/

                await Task.CompletedTask;
            }).Wait();
        }
    }
}