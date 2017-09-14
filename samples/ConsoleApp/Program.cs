using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rabbit.Cloud.Discovery.Client.Builder;
using Rabbit.Cloud.Discovery.Client.Internal;
using Rabbit.Cloud.Extensions.Consul;
using Rabbit.Cloud.Facade;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Builder;
using Rabbit.Extensions.Configuration;
using RC.Cluster;
using RC.Facade.Formatters.Json;
using System;
using System.Threading.Tasks;
using ProxyFactory = Rabbit.Cloud.Facade.ProxyFactory;

namespace ConsoleApp
{
    public class UserMode
    {
        public string Name { get; set; }
        public ushort Age { get; set; }
    }

    public class CustomFilterAttribute : Attribute, IRequestFilter, IResultFilter, IExceptionFilter
    {
        #region Implementation of IRequestFilter

        public void OnRequestExecuting(RequestExecutingContext context)
        {
            Console.WriteLine("OnRequestExecuting");
        }

        public void OnRequestExecuted(RequestExecutedContext context)
        {
            Console.WriteLine("OnRequestExecuted");
        }

        #endregion Implementation of IRequestFilter

        #region Implementation of IResultFilter

        public void OnResultExecuting(ResultExecutingContext context)
        {
            Console.WriteLine("OnResultExecuting");
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            Console.WriteLine("OnResultExecuted");
        }

        #endregion Implementation of IResultFilter

        #region Implementation of IExceptionFilter

        public void OnException(ExceptionContext context)
        {
            Console.WriteLine("OnException");
        }

        #endregion Implementation of IExceptionFilter
    }

    [FacadeClient("userService")]
    [ToHeader("interface", "IUserService"), ToHeader("service", "userService")]
    public interface IUserService
    {
        [RequestMapping("api/User/{id}")]
        [CustomFilter]
        [ToHeader("method", "GetUserAsync"), ToHeader("returnType", "UserMode")]
        Task<UserMode> GetUserAsync(long id, [ToHeader]string version = "1.0.0");

        [RequestMapping("api/User/{id}", "PUT")]
        Task<object> PutUserAsync(long id, [ToForm]UserMode user);
    }

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build()
                .EnableTemplateSupport();

            var services = new ServiceCollection()
                .Configure<RabbitConsulOptions>(configuration.GetSection("RabbitCloud:Consul"))
                .AddConsulDiscovery()
                .AddHighAvailability()
                .AddRandomAddressSelector()
                .AddFacadeCore()
                .AddJsonFormatters()
                .Services
                .BuildServiceProvider();

            var rabbitRequestDelegate = new RabbitApplicationBuilder(services)
                .UseServiceContainer()
                .UseFacade()
                .UseHighAvailability()
                .UseLoadBalance()
                .UseRabbitClient()
                .Build();

            var proxyFactory = new ProxyFactory(rabbitRequestDelegate, services.GetRequiredService<IOptions<FacadeOptions>>());
            var userService = proxyFactory.GetProxy<IUserService>();

            var model = await userService.GetUserAsync(1);
            Console.WriteLine(JsonConvert.SerializeObject(model));

            var result = await userService.PutUserAsync(1, new UserMode
            {
                Age = 30,
                Name = "mk"
            });
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}