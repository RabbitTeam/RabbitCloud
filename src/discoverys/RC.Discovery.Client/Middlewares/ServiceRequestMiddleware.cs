using Rabbit.Cloud.Discovery.Client.Internal;
using RC.Discovery.Client.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Discovery.Client.Middlewares
{
    public class ServiceRequestMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly HttpClient _httpClient;

        public ServiceRequestMiddleware(RabbitRequestDelegate next, HttpClient httpClient)
        {
            _next = next;
            _httpClient = httpClient;
        }

        public async Task Invoke(RabbitContext context)
        {
            var request = context.Request;
            context.Response = new DefaultRabbitResponse(await _httpClient.SendAsync(request));
            await _next(context);
        }
    }
}