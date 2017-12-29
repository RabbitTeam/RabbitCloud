using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private static readonly HttpClient HttpClient;

        static HttpMiddleware()
        {
            HttpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip
            });
        }

        public HttpMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();
            var requestOptions = serviceRequestFeature.RequestOptions;

            var httpRequest = CreateHttpRequestMessage(context);
            HttpResponseMessage httpResponse;

            try
            {
                using (var timeoutCancellationTokenSource = new CancellationTokenSource(requestOptions.ConnectionTimeout.Add(requestOptions.ReadTimeout)))
                    httpResponse = await HttpClient.SendAsync(httpRequest, timeoutCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                throw ExceptionUtilities.ServiceRequestTimeout(httpRequest.RequestUri.ToString());
            }

            await SetResponseAsync(context, httpResponse);

            await _next(context);
        }

        #region Private Method

        private static HttpRequestMessage CreateHttpRequestMessage(IRabbitContext context)
        {
            var request = context.Request;
            var instance = context.Features.Get<IServiceRequestFeature>().ServiceInstance;

            var authority = instance.Port >= 0 ? $"{instance.Host}:{instance.Port}" : instance.Host;
            var url = $"{request.Scheme}://{authority}{request.Path}{request.QueryString}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value.ToArray());
            }

            return httpRequest;
        }

        private static async Task SetResponseAsync(IRabbitContext context, HttpResponseMessage httpResponse)
        {
            var response = context.Response;
            var httpResponseContent = httpResponse.Content;
            foreach (var header in httpResponse.Headers.Concat(httpResponseContent.Headers))
            {
                response.Headers[header.Key] = new StringValues(header.Value.ToArray());
            }

            response.StatusCode = (int)httpResponse.StatusCode;

            var stream = await httpResponseContent.ReadAsStreamAsync();
            response.Body = stream;
        }

        #endregion Private Method
    }
}