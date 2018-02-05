using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Go;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task RequestAsync(GoContext context)
        {
            var rabbitContext = new RabbitContext();

            var rabbitRequest = rabbitContext.Request;

            var goRequest = context.Request;

            rabbitRequest.Scheme = goRequest.Scheme;
            rabbitRequest.Host = goRequest.Host;
            rabbitRequest.Port = goRequest.Port ?? 0;

            rabbitRequest.Path = goRequest.Path;

            rabbitContext.Items["HttpMethod"] = goRequest.Method;

            if (goRequest.Query.Any())
                rabbitRequest.QueryString = QueryHelpers.AddQueryString(string.Empty, goRequest.Query.ToDictionary(i => i.Key, i => i.Value.ToString()));

            foreach (var item in goRequest.Headers)
                rabbitRequest.Headers[item.Key] = item.Value;

            rabbitRequest.Body = goRequest.Body;

            IRabbitClientFeature rabbitClientFeature = new RabbitClientFeature
            {
                RequestOptions = new ServiceRequestOptions
                {
                    Timeout = context.Request.Options.Timeout
                }
            };

            rabbitContext.Features.Set(rabbitClientFeature);

            await _app(rabbitContext);

            await InitializeResponseAsync(context.Response, (HttpResponseMessage)rabbitContext.Response.Body);
        }

        #endregion Implementation of IGoClient

        private static async Task InitializeResponseAsync(GoResponse response, HttpResponseMessage httpResponse)
        {
            response.StatusCode = (int)httpResponse.StatusCode;
            response.Content = await httpResponse.Content.ReadAsStreamAsync();

            void AddHeaders(HttpHeaders headers)
            {
                if (headers == null || !headers.Any())
                    return;

                foreach (var header in headers)
                {
                    if (response.Headers.TryGetValue(header.Key, out var current))
                    {
                        response.Headers[header.Key] = StringValues.Concat(current, new StringValues(header.Value.ToArray()));
                    }
                }
            }

            AddHeaders(httpResponse.Headers);
            AddHeaders(httpResponse.Content?.Headers);
        }
    }
}