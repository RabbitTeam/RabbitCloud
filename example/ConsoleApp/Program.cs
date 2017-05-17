using RabbitCloud.Rpc;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Formatters.Json;
using RabbitCloud.Rpc.NetMQ;
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
            /*            var data=formatter.OutputFormatter.Format(new Request(new Dictionary<string, string>
                        {
                            {"K","V" }
                        })
                        {
                            RequestId = 123,
                            Arguments = new object[] { "123", 1, DateTime.Now },
                            Key = new ServiceKey("test")
                        });
                        var r = formatter.InputFormatter.Format(data);*/

            /*
                        data=ff.OutputFormatter.Format(new Response(r)
                        {
                            Value = DateTime.Now
                        });

                        var rrp = ff.InputFormatter.Format(data);*/
            Task.Run(async () =>
            {
                var jsonRequestFormatter = new JsonRequestFormatter();
                var jsonResponseFormatter = new JsonResponseFormatter();
                IRequestIdGenerator requestIdGenerator=new DefaultRequestIdGenerator();

                IResponseSocketFactory responseSocketFactory = new ResponseSocketFactory();
                var typeCaller = new TypeCaller(new UserService());
                var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);

                Task.Run(() =>
                {
                    var exporter = new NetMqExporter(typeCaller, endPoint,
                        responseSocketFactory, jsonRequestFormatter, jsonResponseFormatter);
                    exporter.Export();
                }).Wait(TimeSpan.FromSeconds(2));

                ICaller caller = new NetMqCaller(endPoint, jsonRequestFormatter, jsonResponseFormatter);

                var userService = new ProxyFactory(requestIdGenerator).GetProxy<IUserService>(caller);

                for (int i = 0; i < 1000; i++)
                {
                    userService.GetName(1);
                }
                Console.WriteLine("ok");
                Console.ReadLine();
            }).Wait();
        }
    }
}