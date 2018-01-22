using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Go.Core
{
    public class AsynchronousMethodHandler
    {
        private readonly IGoClient _client;
        private readonly MethodDescriptor _descriptor;
        private readonly Func<object[], Task<RequestContext>> _requestContextFactory;
        private readonly RequestOptions _options;
        private readonly IDecoder _decoder;

        public AsynchronousMethodHandler(
            IGoClient client,
            MethodDescriptor descriptor,
            Func<object[], Task<RequestContext>> requestContextFactory,
            RequestOptions options,
            IDecoder decoder)
        {
            _client = client;
            _descriptor = descriptor;
            _requestContextFactory = requestContextFactory;
            _options = options;
            _decoder = decoder;
        }

        public async Task<object> InvokeAsync(object[] arguments)
        {
            var requestContext = await _requestContextFactory(arguments);

            var requestMessage = CreateRequestMessage(requestContext);

            var responseMessage = await _client.ExecuteAsync(requestMessage, _options);

            if (_decoder == null)
                return null;

            return await _decoder.DecodeAsync(responseMessage, _descriptor.ReturnType);
        }

        private static string BuildQueryString(RequestContext context)
        {
            if (!context.Query.Any())
                return "?";

            var queryBuilder = new StringBuilder();
            foreach (var item in context.Query)
            {
                queryBuilder
                    .Append("&")
                    .Append(item.Key)
                    .Append("=")
                    .Append(item.Value.ToString());
            }

            var queryString = queryBuilder.Remove(0, 1).Insert(0, '?').ToString();
            return queryString;
        }

        private static void BuildUri(HttpRequestMessage message, RequestContext context)
        {
            var queryString = BuildQueryString(context);

            var url = $"{context.Scheme}://{context.Host}:{context.Port}{context.Path}";

            if (queryString != "?")
            {
                url += queryString;
            }

            message.RequestUri = new Uri(url);
        }

        private static void BuildHeaders(HttpRequestMessage message, RequestContext context)
        {
            var messageHeaders = message.Headers;
            var contentHeaders = message.Content.Headers;

            foreach (var item in context.Headers)
            {
                var key = item.Key;
                var values = item.Value.ToArray();

                if (contentHeaders.TryAddWithoutValidation(key, values))
                    continue;
                messageHeaders.Add(key, values);
            }
        }

        private static void BuildContent(HttpRequestMessage message, RequestContext context)
        {
            message.Content = context.Body != null
                ? new ByteArrayContent(context.Body)
                : new StringContent(string.Empty);

            if (context.Charset != null)
                message.Content.Headers.ContentType.CharSet = context.Charset;
        }

        private static void BuildMethod(HttpRequestMessage message, RequestContext context)
        {
            message.Method = GetHttpMethod(context.Method);
        }

        private static HttpRequestMessage CreateRequestMessage(RequestContext context)
        {
            var message = new HttpRequestMessage();

            BuildMethod(message, context);
            BuildUri(message, context);
            BuildContent(message, context);
            BuildHeaders(message, context);

            return message;
        }

        private static HttpMethod GetHttpMethod(string method, HttpMethod def = null)
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
                    return def ?? HttpMethod.Get;

                default:
                    return new HttpMethod(method);
            }
        }
    }
}