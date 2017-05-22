using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.NetMQ;
using RabbitCloud.Rpc.NetMQ.Internal;
using RabbitCloud.Rpc.Proxy;
using System;
using System.Linq;
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
                    .AddSingleton<ProtocolFactory, ProtocolFactory>()
                    .AddSingleton<FormatterFactory, FormatterFactory>()
                    .AddSingleton<RegistryTableFactory, RegistryTableFactory>();
                

                var model = new CloudApplicationModel
                {
                    Protocols = new[]
                    {
                        new ProtocolConfig
                        {
                            Id = "netmq",
                            Name = "netmq"
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
                
                services
                    .AddSingleton<Class3, Class3>();
                foreach (var serviceConfig in model.Services)
                {
                    services
                        .AddSingleton(Type.GetType(serviceConfig.Interface), Type.GetType(serviceConfig.Implement));
                }
                

                var serviceProvider = services.BuildServiceProvider();


                var class3 = serviceProvider.GetRequiredService<Class3>();

                class3.GetRegistryTable(model.Registrys.FirstOrDefault());
                class3.GetProtocol(model.Protocols.FirstOrDefault());
                foreach (var serviceConfig in model.Services)
                {
                    await class3.Export(serviceConfig);
                }
                var userService = await class3.Referer<IUserService>(model.Referers.FirstOrDefault());

                Console.WriteLine(userService.GetName(1));
                await Task.CompletedTask;
            }).Wait();
            Console.ReadLine();
        }
    }
}