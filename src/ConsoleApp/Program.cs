using org.apache.zookeeper;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Builder;
using RabbitCloud.Registry.Abstractions.Cluster;
using RabbitCloud.Registry.Redis;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Abstractions.Proxy.Castle;
using RabbitCloud.Rpc.Cluster.Internal.Available;
using RabbitCloud.Rpc.Cluster.LoadBalance;
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

        public Task<string> GetServiceName()
        {
            return Task.FromResult(_name);
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

                var containerConfig1 = new ContainerBuilder()
                    .UseAddress(new Url("rabbitcloud://127.0.0.1:9981"))
                    .ConfigurationServices(builder =>
                    {
                        builder
                            .AddService(b =>
                            {
                                b
                                    .Factory<IUserService>(() => new UserService("userService1"));
                            });
                    })
                    .UseProtocol(new RabbitProtocol(new ServerTable(codec), new ClientTable(codec)))
                    .Build();
                var containerConfig2 = new ContainerBuilder()
                    .UseAddress(new Url("rabbitcloud://127.0.0.1:9982"))
                    .ConfigurationServices(builder =>
                    {
                        builder
                            .AddService(b =>
                            {
                                b
                                    .Factory<IUserService>(() => new UserService("userService2"));
                            });
                    })
                    .UseProtocol(new RabbitProtocol(new ServerTable(codec), new ClientTable(codec)))
                    .Build();

                var applicationConfig = new ApplicationBuilder()
                    .UseContainers(new[] { containerConfig1, containerConfig2 })
                    .UseProxyFactory(new CastleProxyFactory())
                    .UseRegistry(registry)
                    .UseProtocol(new RabbitProtocol(new ServerTable(codec), new ClientTable(codec)))
                    .Build();
                await StartApplications(applicationConfig);

                var userService = await GetService<IUserService>(applicationConfig);
                Console.WriteLine(await userService.GetServiceName());
                Console.WriteLine(await userService.GetServiceName());
                Console.WriteLine(await userService.GetServiceName());
                Console.WriteLine(await userService.GetServiceName());
            }).Wait();
            Console.ReadLine();
        }

        private static async Task StartApplications(params ApplicationConfigModel[] applicationConfigs)
        {
            foreach (var applicationConfig in applicationConfigs)
            {
                await StartApplication(applicationConfig);
            }
        }

        private static async Task StartApplication(ApplicationConfigModel applicationConfig)
        {
            foreach (var containerConfig in applicationConfig.Containers)
            {
                var protocol = new RegistryProtocol(applicationConfig.Registry, containerConfig.Protocol, new AvailableCluster(new RoundRobinLoadBalance()));
                foreach (var serviceConfig in containerConfig.ServiceConfigModels)
                {
                    var baseAddress = containerConfig.Address;
                    var url = new Url($"{baseAddress.Scheme}://{baseAddress.Host}:{baseAddress.Port}/{serviceConfig.ServiceKey}");
                    await protocol.Export(new DefaultProvider(() => serviceConfig.ServiceFactory(), url, serviceConfig.ServiceType));
                }
            }
        }

        private static async Task<T> GetService<T>(ApplicationConfigModel applicationConfig, string serviceName = null)
        {
            var protocol = new RegistryProtocol(applicationConfig.Registry, applicationConfig.Protocol, new AvailableCluster(new RoundRobinLoadBalance()));
            var proxyFactory = applicationConfig.ProxyFactory;
            var caller = await protocol.Refer(typeof(T), new Url($"rabbitcloud://temp/{serviceName ?? typeof(T).Name}"));
            var userService = proxyFactory.GetProxy<T>(new RefererInvocationHandler(caller).Invoke);
            return userService;
        }
    }
}