using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Registry.Consul;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.Memory;
using RabbitCloud.Rpc.NetMQ;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                /*                using (var client = new ConsulClient(o =>
                                {
                                    o.Address = new Uri("http://localhost:8500");
                                }))
                                {
                                    foreach (var item in (await client.Agent.Services()).Response)
                                    {
                                        if(item.Value.Service == "rabbitrpc_user")
                                        await client.Agent.ServiceDeregister(item.Value.ID);
                                    }
                                }
                                return;*/
                await RunServer();
                await RunClient();
                Console.WriteLine("1.server");
                Console.WriteLine("2.client");
                var keyword = Console.ReadLine();
                switch (keyword)
                {
                    case "1":
                        {
                        }
                        break;

                    case "2":
                        {
                        }
                        break;
                }
            }).Wait();
        }

        private static async Task RunServer()
        {
            var applicationModel = await CreateApplication("services.json");
        }

        private static async Task RunClient()
        {
            var applicationModel = await CreateApplication("clients.json");
            //从模型中引用 IUserService 服务
            var userService = applicationModel.Referer<IUserService>();
            while (true)
            {
                var s = Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    userService.GetName(1);
                }
                s.Stop();
                Console.WriteLine(s.ElapsedMilliseconds + "ms");
                Console.ReadLine();
/*                
                userService = applicationModel.Referer<IUserService>("test");
                Stopwatch.StartNew();
                for (int i = 0; i < 10000; i++)
                {
                    userService.GetName(1);
                }
                s.Stop();
                Console.WriteLine(s.ElapsedMilliseconds + "ms");
                Console.ReadLine();*/
            }
            do
            {
                //执行调用
                Console.WriteLine(userService.GetName(1));
            } while (Console.ReadLine() != "exit");

            applicationModel.Dispose();
        }

        private static async Task<IApplicationModel> CreateApplication(string configFile)
        {
            #region service registry

            var services = new ServiceCollection();

            services
                .AddLogging()
                .AddRabbitRpc()
                .AddMemoryProtocol()
                .AddJsonFormatter()
                .AddNetMqProtocol()
                .AddConsulRegistryTable();

            services
                .AddRabbitCloud();

            var serviceProvider = services.BuildServiceProvider();

            #endregion service registry

            //从配置文件中加载 ApplicationModelDescriptor
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();
            var descriptor = new ApplicationModelDescriptor();
            configuration.Bind(descriptor);

            var applicationFactory = serviceProvider.GetRequiredService<IApplicationFactory>();

            //根据 ApplicationModelDescriptor 创建程序模型
            var applicationModel = await applicationFactory.CreateApplicationAsync(descriptor);

            return applicationModel;
        }
    }
}