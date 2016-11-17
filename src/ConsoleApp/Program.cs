using org.apache.zookeeper;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions.Builder;
using RabbitCloud.Registry.Abstractions.Cluster;
using RabbitCloud.Registry.Redis;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Abstractions.Proxy.Castle;
using RabbitCloud.Rpc.Cluster.Internal.Available;
using RabbitCloud.Rpc.Default;
using RabbitCloud.Rpc.Default.Service;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class UserModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public interface IUserService
    {
        void Test();

        Task Test2();

        Task<string> Test3();

        Task Test4(UserModel model);

        Task<string> GetServiceName();
    }

    internal class UserService : IUserService
    {
        private readonly string _name;

        public UserService(string name)
        {
            _name = name;
        }

        public void Test()
        {
            Console.WriteLine(_name);
        }

        public Task Test2()
        {
            Console.WriteLine("test2");
            return Task.CompletedTask;
        }

        public Task<string> Test3()
        {
            //            Console.WriteLine("test3");
            return Task.FromResult("3");
        }

        public Task Test4(UserModel model)
        {
            Console.WriteLine(model.Name);
            return Task.CompletedTask;
        }

        public Task<string> GetServiceName()
        {
            return Task.FromResult(_name);
        }
    }

    internal class MyClass : Watcher
    {
        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public override Task process(WatchedEvent @event)
        {
            return Task.CompletedTask;
        }

        #endregion Overrides of Watcher
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                ICodec codec = new RabbitCodec();
                var registry = new RedisRegistryFactory().GetRegistry(new Url("redis://?ConnectionString=127.0.0.1:6379&database=-1&application=test"));
                var rabbitProtocol = new RabbitProtocol(new ServerTable(codec), new ClientTable(codec));
                IProtocol protocol = new RegistryProtocol(registry, rabbitProtocol, new AvailableCluster());

                var hostBuilder = new HostBuilder();

                hostBuilder
                    .SetProtocol("rabbitcloud")
                    .SetAddress("127.0.0.1", 9981)
                    .AddService(serviceConfig =>
                    {
                        serviceConfig
                            .Factory<IUserService>(() => new UserService("service1"))
                            .Id("userService1");
                    })
                    .AddService(serviceConfig =>
                    {
                        serviceConfig
                            .Factory<IUserService>(() => new UserService("service2"))
                            .Id("userService2");
                    });

                await StartHost(protocol, hostBuilder);
                /*                hostBuilder = new HostBuilder();

                                hostBuilder
                                    .SetProtocol("rabbitcloud")
                                    .SetAddress("127.0.0.1", 9982)
                                    .AddService(serviceConfig =>
                                    {
                                        serviceConfig
                                            .Factory<IUserService>(() => new UserService("service2"));
                                    });

                                await StartHost(protocol, hostBuilder);*/

                var userService = await GetService<IUserService>(protocol, "userService1");
                Console.WriteLine(await userService.GetServiceName());
                Console.WriteLine(await userService.GetServiceName());
                userService = await GetService<IUserService>(protocol, "userService2");
                Console.WriteLine(await userService.GetServiceName());
                Console.WriteLine(await userService.GetServiceName());
            }).Wait();
            Console.ReadLine();
        }

        private static async Task StartHost(IProtocol protocol, HostBuilder hostBuilder)
        {
            var hostConfig = hostBuilder.Build();
            foreach (var serviceConfig in hostConfig.ServiceConfigModels)
            {
                var url = new Url($"{hostConfig.Protocol}://{hostConfig.Address}/{serviceConfig.ServiceId}");
                await protocol.Export(new DefaultProvider(() => serviceConfig.ServiceFactory(), url, serviceConfig.ServiceType));
            }
        }

        private static async Task<T> GetService<T>(IProtocol protocol, string serviceName)
        {
            var caller = await protocol.Refer(typeof(T), new Url($"rabbitcloud://temp/{serviceName}"));
            IProxyFactory proxyFactory = new CastleProxyFactory();
            var userService = proxyFactory.GetProxy<T>(new RefererInvocationHandler(caller).Invoke);
            return userService;
        }
    }
}