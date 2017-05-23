using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Registry.Consul.Config;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.Formatters.Json.Config;
using RabbitCloud.Rpc.NetMQ;
using RabbitCloud.Rpc.NetMQ.Config;
using RabbitCloud.Rpc.NetMQ.Internal;
using RabbitCloud.Rpc.Proxy;
using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var services = new ServiceCollection();

                services.AddLogging();

                services
                    .AddSingleton<IRequestFormatter, JsonRequestFormatter>()
                    .AddSingleton<IResponseFormatter, JsonResponseFormatter>();

                services
                    .AddSingleton<IRequestIdGenerator, DefaultRequestIdGenerator>()
                    .AddScoped<IProxyFactory, ProxyFactory>();

                services
                    .AddSingleton<IRouterSocketFactory, RouterSocketFactory>()
                    .AddSingleton(new NetMqPollerHolder())
                    .AddSingleton<NetMqProtocol, NetMqProtocol>();

                services
                    .AddSingleton<IProtocolFactory, DefaultProtocolFactory>()
                    .AddSingleton<IFormatterFactory, DefaultFormatterFactory>()
                    .AddSingleton<IRegistryTableFactory, DefaultRegistryTableFactory>()
                    .AddScoped<IApplicationFactory, DefaultApplicationFactory>();

                services
                    .AddSingleton<IRegistryTableProvider, ConsulRegistryTableProvider>()
                    .AddSingleton<IProtocolProvider, NetMqProtocolProvider>()
                    .AddSingleton<IFormatterProvider, JsonFormatterProvider>();

                var descriptor = new ApplicationModelDescriptor
                {
                    Protocols = new[]
                    {
                        new ProtocolConfig
                        {
                            Id = "netmq",
                            Name = "netmq",
                            Formatter = "json"
                        }
                    },
                    Registrys = new[]
                    {
                        new RegistryConfig
                        {
                            Address = "http://localhost:8500",
                            Name = "consul",
                            Protocol = "consul"
                        }
                    },
                    Services = new[]
                    {
                        new ServiceConfig
                        {
                            Export = "netmq://192.168.5.26:9999",
                            Id = "userService",
                            Interface = typeof(IUserService).AssemblyQualifiedName,
                            Implement = typeof(UserService).AssemblyQualifiedName,
                            Registry = "consul",
                            Group = "user"
                        }
                    },
                    Referers = new[]
                    {
                        new RefererConfig
                        {
                            Id = "userService",
                            Interface = typeof(IUserService).AssemblyQualifiedName,
                            Protocol = "netmq",
                            Registry = "consul",
                            Group = "user"
                        }
                    }
                };

                foreach (var serviceConfig in descriptor.Services)
                {
                    services
                        .AddSingleton(Type.GetType(serviceConfig.Interface), Type.GetType(serviceConfig.Implement));
                }

                var serviceProvider = services.BuildServiceProvider();

                var applicationFactory = serviceProvider.GetRequiredService<IApplicationFactory>();

                var applicationModel = await applicationFactory.CreateApplicationAsync(descriptor);
                var userService = applicationModel.Referer<IUserService>("userService");

                Console.WriteLine(userService.GetName(1));
                await Task.CompletedTask;
            }).Wait();
            Console.ReadLine();
        }
    }
}