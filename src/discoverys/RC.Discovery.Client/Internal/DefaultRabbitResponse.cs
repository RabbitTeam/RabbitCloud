using RC.Discovery.Client.Abstractions;
using System.Net.Http;

namespace Rabbit.Cloud.Discovery.Client.Internal
{
    public class DefaultRabbitResponse : RabbitResponse
    {
        public DefaultRabbitResponse()
        {
        }

        public DefaultRabbitResponse(HttpResponseMessage responseMessage)
        {
            Content = responseMessage.Content;
            foreach (var item in responseMessage.Headers)
            {
                Headers.Add(item.Key, item.Value);
            }
            ReasonPhrase = responseMessage.ReasonPhrase;
            RequestMessage = responseMessage.RequestMessage;
            StatusCode = responseMessage.StatusCode;
            Version = responseMessage.Version;
        }
    }
}