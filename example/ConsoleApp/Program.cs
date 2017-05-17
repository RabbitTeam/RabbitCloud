using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.NetMQ;
using RabbitCloud.Rpc.NetMQ.Internal;
using RabbitCloud.Rpc.Proxy;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public interface IUserService
    {
        string GetName(long id);

        string GetName(long id, string name);

        Task Test();

        Task<string> Test2();

        void Test3();
    }

    public class UserService : IUserService
    {
        public string GetName(long id)
        {
            return "123";
        }

        public string GetName(long id, string name)
        {
            return id + name;
        }

        public Task Test()
        {
            Console.WriteLine("test");
            return Task.CompletedTask;
        }

        public Task<string> Test2()
        {
            return Task.FromResult("asfasd");
        }

        public void Test3()
        {
            Console.WriteLine("test3");
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var jsonRequestFormatter = new JsonRequestFormatter();
                var jsonResponseFormatter = new JsonResponseFormatter();

                IRequestIdGenerator requestIdGenerator = new DefaultRequestIdGenerator();

                var proxyFactory = new ProxyFactory(requestIdGenerator);

                IResponseSocketFactory responseSocketFactory = new ResponseSocketFactory();
                var netMqPollerHolder = new NetMqPollerHolder();

                IProtocol protocol = new NetMqProtocol(responseSocketFactory, jsonRequestFormatter, jsonResponseFormatter, netMqPollerHolder);

                var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);

                protocol.Export(new ExportContext
                {
                    Caller = new TypeCaller(new UserService()),
                    EndPoint = endPoint
                });

                var caller = protocol.Refer(new ReferContext
                {
                    EndPoint = endPoint
                });

                var userService = proxyFactory.GetProxy<IUserService>(caller);

                for (var i = 0; i < 10000; i++)
                {
                    Console.WriteLine(userService.GetName(1));
                    Console.WriteLine(i);
                }

                await Task.CompletedTask;
            }).Wait();
        }
    }
}