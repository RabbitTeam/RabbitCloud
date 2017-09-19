using Microsoft.Extensions.DependencyInjection;
using RC.Abstractions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Middlewares
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
            var responseMessage = context.Response.ResponseMessage = await httpClient.SendAsync(request.RequestMessage);

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception(responseMessage.ToString());
        }
    }
}