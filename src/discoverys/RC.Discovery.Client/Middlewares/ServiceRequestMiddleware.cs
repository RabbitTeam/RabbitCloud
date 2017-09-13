using Microsoft.Extensions.DependencyInjection;
using RC.Discovery.Client.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Client.Middlewares
{
    public class ServiceRequestMiddleware
    {
        public ServiceRequestMiddleware(RabbitRequestDelegate _)
        {
        }

        public async Task Invoke(RabbitContext context)
        {
            var httpClient = context.RequestServices.GetRequiredService<HttpClient>();
            var request = context.Request;
            context.Response.ResponseMessage = await httpClient.SendAsync(request.RequestMessage);
        }
    }
}