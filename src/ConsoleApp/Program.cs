using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Config.Abstractions.Config.Internal;
using RabbitCloud.Config.Abstractions.Internal;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Registry.Redis;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Abstractions.Proxy.Castle;
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

    public class Program
    {
        internal class ServiceLocator : IProtocolLocator, IRegistryLocator
        {
            private IProtocol _protocol;
            private IRegistry _registry;

            #region Implementation of IProtocolLocator

            public Task<IProtocol> GetProtocol(string name)
            {
                if (_protocol != null)
                    return Task.FromResult(_protocol);
                var codec = new RabbitCodec();
                _protocol = new RabbitProtocol(new ServerTable(codec), new ClientTable(codec));
                return Task.FromResult(_protocol);
            }

            #endregion Implementation of IProtocolLocator

            #region Implementation of IRegistryLocator

            public Task<IRegistry> GetRegistry(Url url)
            {
                if (_registry != null)
                    return Task.FromResult(_registry);
                _registry = new RedisRegistryFactory().GetRegistry(url);
                return Task.FromResult(_registry);
            }

            #endregion Implementation of IRegistryLocator
        }

        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var services = new ServiceCollection();
                services.AddSingleton<IProxyFactory, CastleProxyFactory>();
                services.AddSingleton<IProtocolLocator, ServiceLocator>();
                services.AddSingleton<IRegistryLocator, ServiceLocator>();
                services.AddSingleton<IUserService>(new UserService("test"));
                var serviceContainer = services.BuildServiceProvider();
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("rabbitcloud.json")
                    .Build();

                var applicationBuilder = new ConfigurationApplicationBuilder(serviceContainer, configuration);
                var application = await applicationBuilder.Build();

                var userService = application.GetReference<IUserService>();

                Console.WriteLine(await userService.GetServiceName());
                Console.WriteLine(await userService.GetServiceName());
            }).Wait();
            Console.ReadLine();
        }
    }
}