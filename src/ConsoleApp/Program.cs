using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Proxy.Castle;
using RabbitCloud.Rpc.Abstractions.Serialization.Implementation;
using RabbitCloud.Rpc.Default;
using RabbitCloud.Rpc.Default.Service;
using System;
using System.Threading;
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
        private async Task Run()
        {
            Parallel.For(0, 100, i =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            });
        }

        public static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var services = new ServiceCollection();
                services.AddTransient<IUserService, UserService>();
                var provider = services.BuildServiceProvider();
                var serializer = new JsonSerializer();
                ICodec codec = new DefaultCodec(serializer);
                var proxyFactory = new CastleProxyFactory();


                var url = Url.Create("rabbit://127.0.0.1:9981/test");

                var localInvoker = proxyFactory.GetInvoker(() => provider.GetRequiredService<IUserService>(), url);
                var protocol = new RabbitProtocol(new ServerTable(codec), new ClientTable(codec));
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