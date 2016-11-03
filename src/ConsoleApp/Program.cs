using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
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
    }

    internal class UserService : IUserService
    {
        public void Test()
        {
            Console.WriteLine("test");
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
        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var services = new ServiceCollection();
                services.AddTransient<IUserService, UserService>();
                services.AddLogging();
                var provider = services.BuildServiceProvider();

                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();

                //                var serializer = new JsonSerializer();
                var proxyFactory = new CastleProxyFactory();

                IProtocol protocol;
                Url url;
                {
                    ICodec codec = new DefaultCodec();
                    url = Url.Create("rabbit://127.0.0.1:9981/test");
                    protocol = new RabbitProtocol(new ServerTable(codec), new ClientTable(codec), logger);
                }

                /*//http协议
                {
                    ICodec codec = new HttpCodec(serializer);
                    url = Url.Create("http://127.0.0.1:9981/test");
                    protocol = new HttpProtocol(new ServiceTable(codec), codec);
                }*/

                var localInvoker = proxyFactory.GetInvoker(() => provider.GetRequiredService<IUserService>(), url);

                protocol.Export(localInvoker);
                var remoteInvoker = protocol.Refer(url);

                var userService = proxyFactory.GetProxy<IUserService>(remoteInvoker);

                await userService.Test4(new UserModel
                {
                    Name = "aa"
                });
            }).Wait();
        }
    }
}