using org.apache.zookeeper;
using RabbitCloud.Abstractions;
using RabbitCloud.Registry.ZooKeeper;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Protocol;
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

    internal class MyClass : Watcher
    {
        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        public override Task process(WatchedEvent @event)
        {
            return Task.CompletedTask;
        }

        #endregion Overrides of Watcher
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

                var registry = new ZookeeperRegistryFactory().GetRegistry(new Url("zookeeper://172.18.20.132:2181"));

                await registry.Register(url);
                foreach (var u in await registry.Discover(url))
                {
                    Console.WriteLine(u);
                }

                /*                var referer = protocol.Refer(typeof(IUserService), url);

                                IProxyFactory factory = new CastleProxyFactory();

                                var invocationHandler = new RefererInvocationHandler(referer);
                                var userService = factory.GetProxy<IUserService>(invocationHandler.Invoke);

                                userService.Test();
                                await userService.Test2();
                                Console.WriteLine(await userService.Test3());
                                await userService.Test4(new UserModel
                                {
                                    Id = 1,
                                    Name = "test"
                                });*/

                await Task.CompletedTask;
            }).Wait();
        }
    }
}