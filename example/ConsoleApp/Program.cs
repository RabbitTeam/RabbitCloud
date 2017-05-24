using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Config;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Registry.Consul;
using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.NetMQ;
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
                Console.WriteLine("1.server");
                Console.WriteLine("2.client");
                var keyword = Console.ReadLine();
                switch (keyword)
                {
                    case "1":
                        {
                            await RunServer();
                        }
                        break;

                    case "2":
                        {
                            await RunClient();
                        }
                        break;
                }
            }).Wait();
        }

        private static async Task RunServer()
        {
            await CreateApplication("services.json");
        }

        private static async Task RunClient()
        {
            var applicationModel = await CreateApplication("clients.json");
            //从模型中引用 IUserService 服务
            var userService = applicationModel.Referer<IUserService>();
            while (true)
            {
                //执行调用
                Console.WriteLine(userService.GetName(1));
                Console.ReadLine();
            }
        }

        private static async Task<ApplicationModel> CreateApplication(string configFile)
        {
            #region service registry

            var services = new ServiceCollection();

            services
                .AddLogging()
                .AddRabbitRpc()
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