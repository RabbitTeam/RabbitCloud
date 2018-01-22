using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Go.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go
{
    public class RabbitCloudGoClient : IGoClient
    {
        private readonly RabbitRequestDelegate _app;

        public RabbitCloudGoClient(RabbitRequestDelegate app)
        {
            _app = app;
        }

        #region Implementation of IGoClient

        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request, RequestOptions options)
        {
            var rabbitContext = new RabbitContext();

            var rabbitRequest = rabbitContext.Request;

            var requestUri = request.RequestUri;
            rabbitRequest.Host = requestUri.Host;
            //            rabbitRequest.Headers= requestUri.
            rabbitRequest.Port = requestUri.Port;

            var pathAndQuery = requestUri.PathAndQuery;

            var queryStartIndex = pathAndQuery.IndexOf('?');

            if (queryStartIndex != -1)
            {
                rabbitRequest.Path = pathAndQuery.Substring(0, queryStartIndex);
                rabbitRequest.QueryString = pathAndQuery.Substring(queryStartIndex);
            }
            else
                rabbitRequest.Path = pathAndQuery;

            rabbitRequest.Scheme = requestUri.Scheme;

            foreach (var item in request.Headers)
                rabbitRequest.Headers[item.Key] = new StringValues(item.Value.ToArray());

            rabbitRequest.Body = request.Content;

            await _app(rabbitContext);

            return (HttpResponseMessage)rabbitContext.Response.Body;
        }

        #endregion Implementation of IGoClient
    }
}