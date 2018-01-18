using Rabbit.Go.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go.Internal
{
    public interface IHttpRequestFeature
    {
        HttpClient HttpClient { get; set; }
        string Url { get; set; }
        HttpContent Content { get; set; }
        string Method { get; set; }
        Version Version { get; set; }
    }

    public interface IHttpResponseFeature
    {
        HttpContent Content { get; set; }
        bool IsSuccessStatusCode { get; set; }
        string ReasonPhrase { get; set; }
        HttpStatusCode StatusCode { get; set; }
        Version Version { get; set; }
    }

    public class HttpGoInvoker : IGoRequestInvoker
    {
        private readonly RequestContext _requestContext;

        public HttpGoInvoker(RequestContext requestContext)
        {
            _requestContext = requestContext;
        }

        #region Implementation of IGoRequestInvoker

        public async Task InvokeAsync()
        {
            var httpRequestFeature = _requestContext.RabbitContext.Features.Get<IHttpRequestFeature>();

            var request = _requestContext.RabbitContext.Request;

            var method = GetHttpMethod(httpRequestFeature.Method, HttpMethod.Get);

            //            var url=$"{request.Scheme}://{request.Host}:{request.Port}{request.Path}{request.QueryString}";
            var url = httpRequestFeature.Url;
            var requestMessage = new HttpRequestMessage(method, url);

            // set headers
            foreach (var requestHeader in request.Headers)
                requestMessage.Headers.Add(requestHeader.Key, requestHeader.Value.ToArray());

            if (httpRequestFeature.Content != null)
                requestMessage.Content = httpRequestFeature.Content;

            if (httpRequestFeature.Version != null)
                requestMessage.Version = httpRequestFeature.Version;

            var httpClient = httpRequestFeature.HttpClient;

            var responseMessage = await httpClient.SendAsync(requestMessage);

            var httpResponseFeature = _requestContext.RabbitContext.Features.Get<IHttpResponseFeature>();

            httpResponseFeature.Content = responseMessage.Content;
        }

        #endregion Implementation of IGoRequestInvoker

        private string TakeHeader(string key)
        {
            var headers = _requestContext.RabbitContext.Request.Headers;
            if (!headers.TryGetValue(key, out var values))
                return null;

            headers.Remove(key);

            return values.ToString();
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