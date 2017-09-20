using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Internal;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public static class RabbitCloudClientHttpExtensions
    {
        public static Task<HttpResponseMessage> GetAsync(this IRabbitCloudClient rabbitCloudClient, string url)
        {
            return rabbitCloudClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
        }

        public static Task<HttpResponseMessage> PostAsync(this IRabbitCloudClient rabbitCloudClient, string url, HttpContent content)
        {
            return rabbitCloudClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            });
        }

        public static async Task<HttpResponseMessage> SendAsync(this IRabbitCloudClient rabbitCloudClient, HttpRequestMessage requestMessage)
        {
            var context = new DefaultRabbitContext();
            context.Request.RequestMessage = requestMessage;

            await rabbitCloudClient.RequestAsync(context);

            return context.Response.ResponseMessage;
        }
    }
}