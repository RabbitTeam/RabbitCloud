using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Abstractions.Proxy.Castle;
using RabbitCloud.Rpc.Default;
using RabbitCloud.Rpc.Default.Service;
using RabbitCloud.Rpc.Default.Utils;
using System;
using System.Linq;
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
            var url = new Url("rabbitrpc://127.0.0.1:9981/test/a?a=1&b=2");
            Task.Run(async () =>
            {
                ICodec codec = new RabbitCodec();
                IProtocol protocol = new RabbitProtocol(new ServerTable(codec), new ClientTable(codec));

                protocol.Export(new DefaultProvider(() => new UserService(), url, typeof(IUserService)), url);
                var referer = protocol.Refer(typeof(IUserService), url);

                IProxyFactory factory = new CastleProxyFactory();

                var userService = factory.GetProxy<IUserService>(async (proxy, method, ag) =>
                {
                    var response = await referer.Call(new DefaultRequest
                    {
                        Arguments = ag,
                        InterfaceName = "",
                        MethodName = method.Name,
                        ParamtersType = method.GetParameters().Select(i => i.ParameterType.FullName).ToArray(),
                        RequestId = MessageIdGenerator.GeneratorId()
                    });

                    return response.Result;
                });

                userService.Test();
                await userService.Test2();
                Console.WriteLine(await userService.Test3());
                await userService.Test4(new UserModel
                {
                    Id = 1,
                    Name = "test"
                });

                await Task.CompletedTask;
            }).Wait();
        }
    }
}