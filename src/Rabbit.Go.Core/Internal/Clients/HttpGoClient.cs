using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Rabbit.Go.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Rabbit.Go.Core
{
    public class HttpGoClient : IGoClient
    {
        private readonly HttpClient _httpClient;

        public HttpGoClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Implementation of IGoClient

        public async Task RequestAsync(GoContext context)
        {
            var httpRequest = CreateHttpRequestMessage(context.Request);
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            await InitializeResponseAsync(context.Response, httpResponse);
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

        private static string BuildUrl(GoRequest request)
        {
            var url = $"{request.Scheme}://{request.Host}";
            if (request.Port.HasValue)
                url += ":" + request.Port.Value;

            url += $"{request.Path}";

            if (request.Query.Any())
                url = QueryHelpers.AddQueryString(url, request.Query.ToDictionary(i => i.Key, i => i.Value.ToString()));

            return url;
        }

        private static HttpRequestMessage CreateHttpRequestMessage(GoRequest request)
        {
            var requestUrl = BuildUrl(request);
            var httpRequest = new HttpRequestMessage(GetHttpMethod(request.Method, HttpMethod.Get), requestUrl);

            if (request.Body != null)
                httpRequest.Content = new StreamContent(request.Body);

            foreach (var header in request.Headers)
            {
                var name = header.Key;
                var value = header.Value.ToString();
                if (httpRequest.Headers.TryAddWithoutValidation(name, value))
                    continue;

                if (httpRequest.Content == null)
                    httpRequest.Content = new StringContent(string.Empty);

                httpRequest.Content.Headers.Add(name, value);
            }

            return httpRequest;
        }

        private static HttpMethod GetHttpMethod(string method, HttpMethod def)
        {
            switch (method?.ToLower())
            {
                case "delete":
                    return HttpMethod.Delete;

                case "get":
                    return HttpMethod.Get;

                case "head":
                    return HttpMethod.Head;

                case "options":
                    return HttpMethod.Options;

                case "post":
                    return HttpMethod.Post;

                case "put":
                    return HttpMethod.Put;

                case "trace":
                    return HttpMethod.Trace;

                case null:
                case "":
                    return def;

                default:
                    return new HttpMethod(method);
            }
        }
    }
}