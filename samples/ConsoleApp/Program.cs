using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rabbit.Cloud;
using Rabbit.Cloud.Builder;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Cluster;
using Rabbit.Cloud.Extensions.Consul;
using Rabbit.Cloud.Facade;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Builder;
using Rabbit.Extensions.Configuration;
using RC.Facade.Formatters.Json;
using System;
using System.Threading.Tasks;

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
        Task<object> PutUserAsync(long id, UserMode model);
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

            var serviceCollection = new ServiceCollection();

            var rabbitCloudClient = serviceCollection
                .BuildRabbitCloudClient((appServices, hostingServiceProvider) => appServices
                        .AddRabbitCloudCore()
                        .AddConsulDiscovery(configuration)
                        .AddHighAvailability()
                        .AddRandomServiceInstanceChoose()
                        .AddFacadeCore()
                        .AddJsonFormatters()
                        .Services
                        .BuildServiceProvider(),
                    app =>
                    {
                        app
                            .UseServiceContainer()
                            .UseFacade()
                            .UseHighAvailability()
                            .UseLoadBalance()
                            .UseRabbitClient();
                    });

            var services = serviceCollection.InjectionFacadeClient(rabbitCloudClient).BuildServiceProvider();

            var userService = services.GetRequiredService<IUserService>();

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