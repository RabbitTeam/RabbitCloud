using Cowboy.Sockets.Tcp.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Extensions;
using RabbitCloud.Rpc.Abstractions.Features;
using RabbitCloud.Rpc.Abstractions.Hosting.Server;
using RabbitCloud.Rpc.Abstractions.Hosting.Server.Features;
using RabbitCloud.Rpc.Abstractions.Hosting.Server.Middlewares;
using RabbitCloud.Rpc.Default;
using RabbitCloud.Rpc.Default.Middlewares;
using System;
using System.Linq;
using System.Net;
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
                //server
                {
                    var server = new RabbitRpcServer();
                    const string host = "localhost";
                    const int port = 9981;
                    var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
                    serverAddressesFeature.Addresses.Add($"{host}:{port}");

                    IServiceCollection services = new ServiceCollection();
                    services.AddSingleton<IUserService, UserService>();
                    var applicationServices = services.BuildServiceProvider();
                    IRpcApplicationBuilder applicationBuilder = new RpcApplicationBuilder(applicationServices);

                    applicationBuilder.UseCodec(new RabbitCodec());
                    applicationBuilder.UseMiddleware<InitializationRequestMiddleware>();

                    applicationBuilder.RegisterService("test", async ar => await Task.FromResult(DateTime.Now));

                    applicationBuilder.RegisterService<IUserService>();

                    applicationBuilder.UseMiddleware<ResponseMiddleware>();

                    var rpcApplication = new RpcApplication(applicationBuilder.Build());
                    server.Start(rpcApplication);
                }

                //client
                {
                    var codec = new RabbitCodec();
                    var client = new TcpSocketSaeaClient(IPAddress.Parse("127.0.0.1"), 9981,
                        async (c, data, offset, count) =>
                        {
                            var b = data.Skip(offset).Take(count).ToArray();
                            var response = codec.Decode(b, typeof(IRpcResponseFeature));
                            await Task.CompletedTask;
                        });
                    await client.Connect();
                    await client.SendAsync((byte[])codec.Encode(new RpcRequestFeature
                    {
                        ServiceId = "ConsoleApp.IUserService.Test3",
                        Body = new object[0]
                    }));
                }
                await Task.CompletedTask;
            }).Wait();
            Console.ReadLine();
        }
    }
}