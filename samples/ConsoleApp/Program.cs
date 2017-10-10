using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Extensions;
using Rabbit.Cloud.Client.Http;
using Rabbit.Cloud.Client.Middlewares;
using Rabbit.Cloud.Cluster;
using Rabbit.Cloud.Extensions.Consul;
using Rabbit.Cloud.Facade;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Builder;
using Rabbit.Cloud.Facade.Formatters.Json;
using Rabbit.Cloud.Facade.Internal;
using Rabbit.Extensions.Configuration;
using Rabbit.Extensions.DependencyInjection;
using System;
using System.IO;
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
    [ToHeader("interface", "IUserService"), ToHeader("service", "userService"), ToHeader("rabbit.chooser", "RoundRobin")]
    public interface IUserService
    {
        [RequestMapping("api/User/{id}")]
        [CustomFilter]
        [ToHeader("method", "GetUserAsync"), ToHeader("returnType", "UserMode")]
        Task<UserMode> GetUserAsync(long id, [ToHeader]string version = "1.0.0");

        [RequestMapping("api/User/{id}", "PUT")]
        Task<object> PutUserAsync(long id, UserMode model);
    }

    public class Program
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
                .AddLogging()
                .AddRabbitCloudCore()
                .AddConsulDiscovery(configuration)
                .AddServiceInstanceChoose()
                .AddFacadeCore()
                .AddJsonFormatters()
                .RabbitBuilder.Services
                .AddServiceExtensions()
                .BuildRabbitServiceProvider();

            IRabbitApplicationBuilder app = new RabbitApplicationBuilder(services);

            app
                .UseMiddleware<RequestServicesContainerMiddleware>()
                .UseFacade()
                .UseHighAvailability()
                .UseLoadBalance()
                .UseMiddleware<HttpServiceMiddleware>();

            var invoker = app.Build();

            var proxyFactory = new ProxyFactory(services);

            var userService = proxyFactory.GetProxy<IUserService>(invoker);

            var user = await userService.GetUserAsync(1);
            return;

            var rabbitContext = new DefaultRabbitContext();

            var request = rabbitContext.Request;
            request.RequestUri = new Uri("http://userService/api/User/1");

            await invoker(rabbitContext);

            var response = rabbitContext.Response;

            Console.WriteLine(new StreamReader(response.Body).ReadToEnd());
        }
    }
}