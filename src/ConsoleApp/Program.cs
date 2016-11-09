using Cowboy.Sockets.Tcp.Client;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Extensions;
using RabbitCloud.Rpc.Abstractions.Hosting.Server;
using RabbitCloud.Rpc.Abstractions.Hosting.Server.Features;
using RabbitCloud.Rpc.Default;
using System;
using System.Net;
using System.Text;
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
                var server = new RabbitRpcServer();
                const string host = "localhost";
                const int port = 9981;
                var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
                serverAddressesFeature.Addresses.Add($"{host}:{port}");

                IRpcApplicationBuilder applicationBuilder = new RpcApplicationBuilder(null);
                applicationBuilder.Use(async (context, next) =>
                {
                    await next.Invoke();
                });

                var rpcApplication = new RpcApplication(applicationBuilder.Build());
                server.Start(rpcApplication);

                TcpSocketSaeaClient client = new TcpSocketSaeaClient(IPAddress.Parse("127.0.0.1"), 9981,
                    async (c, data, offset, count) =>
                    {
                    });
                await client.Connect();
                await client.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                {
                    Path = "/test/1",
                    QueryString = "a=1&b=2",
                    Scheme = "rabbit",
                    Body = new
                    {
                        Arguments = new[]
                        {
                            new
                            {
                                Type=typeof(string).AssemblyQualifiedName,
                                Content="123"
                            }
                        }
                    }
                })));
                await Task.CompletedTask;
            }).Wait();
            Console.ReadLine();
        }
    }
}