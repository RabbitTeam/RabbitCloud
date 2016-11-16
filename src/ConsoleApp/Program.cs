using org.apache.zookeeper;
using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Registry.Abstractions.Cluster;
using RabbitCloud.Registry.Redis;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Internal;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Abstractions.Proxy;
using RabbitCloud.Rpc.Abstractions.Proxy.Castle;
using RabbitCloud.Rpc.Cluster.Internal.Available;
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
        private readonly string _name;

        public UserService(string name)
        {
            _name = name;
        }

        public void Test()
        {
            Console.WriteLine(_name);
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
            var url1 = new Url("rabbitrpc://127.0.0.1:9981/test/a?a=1&b=2");
            var url2 = new Url("rabbitrpc://127.0.0.1:9982/test/a?a=1");
            Task.Run(async () =>
            {
                ICodec codec = new RabbitCodec();
                IRegistry registry = new RedisRegistryFactory().GetRegistry(new Url("redis://?ConnectionString=127.0.0.1:6379&database=-1&application=test"));
                var rabbitProtocol = new RabbitProtocol(new ServerTable(codec), new ClientTable(codec));
                IProtocol protocol = new RegistryProtocol(registry, rabbitProtocol, new AvailableCluster());

                var exporter1=await protocol.Export(new DefaultProvider(() => new UserService("userService1"), url1, typeof(IUserService)));
                var exporter2 = await protocol.Export(new DefaultProvider(() => new UserService("userService2"), url2, typeof(IUserService)));

                var referer = await protocol.Refer(typeof(IUserService), url1);

                IProxyFactory proxyFactory = new CastleProxyFactory();
                var userServiceProxy = proxyFactory.GetProxy<IUserService>(new RefererInvocationHandler(referer).Invoke);
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();

                exporter2.Dispose();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();
                userServiceProxy.Test();


                Console.WriteLine(await userServiceProxy.Test3());
            }).Wait();
            Console.ReadLine();
        }
    }
}