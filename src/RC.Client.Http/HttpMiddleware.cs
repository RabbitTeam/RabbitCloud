using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var requestFeature = context.Features.Get<IServiceRequestFeature>();
            var instance = requestFeature.GetServiceInstance();
            var codec = requestFeature.Codec;

            var authority = instance.Port >= 0 ? $"{instance.Host}:{instance.Port}" : instance.Host;
            var url = $"{request.Scheme}://{authority}{request.Path}";
            if (request.QueryString.Length > 1)
                url += request.QueryString;

            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(url)
            };

            var method = context.Items.TryGetValue("HttpMethod", out var httpMethod) ? httpMethod.ToString() : null;
            httpRequest.Method = GetHttpMethod(method, HttpMethod.Get);

            if (request.Body != null)
            {
                context.Request.Body = codec.Encode(context.Request.Body);
                HttpContent httpContent;
                if (request.Body is string content)
                {
                    httpContent = new StringContent(content);
                }
                else if (request.Body is IEnumerable<byte> data)
                {
                    httpContent = new ByteArrayContent(data.ToArray());
                }
                else if (request.Body is Stream stream)
                {
                    httpContent = new StreamContent(stream);
                }
                else
                {
                    throw new Exception("不支持的Body类型。");
                }
                httpRequest.Content = httpContent;

                var headers = httpContent.Headers;

                if (request.Headers.ContainsKey("Content-Type"))
                    headers.ContentType = null;

                foreach (var header in request.Headers)
                {
                    headers.Add(header.Key, header.Value.ToArray());
                }

                if (headers.ContentType == null)
                    headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            else
            {
                var headers = httpRequest.Headers;
                foreach (var header in request.Headers)
                {
                    headers.Add(header.Key, header.Value.ToArray());
                }
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

        private static async Task SetResponseAsync(IRabbitContext context, HttpResponseMessage httpResponse)
        {
            var requestFeature = context.Features.Get<IServiceRequestFeature>();
            var response = context.Response;

            try
            {
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var requestDetailedException = new HttpRequestDetailedException(httpResponse);
                    throw requestDetailedException;
                }
            }
            catch (Exception e)
            {
                throw ExceptionUtilities.ServiceRequestFailure(requestFeature.ServiceName, (int)httpResponse.StatusCode, e);
            }

            var httpResponseContent = httpResponse.Content;
            foreach (var header in httpResponse.Headers.Concat(httpResponseContent.Headers))
            {
                response.Headers[header.Key] = new StringValues(header.Value.ToArray());
            }

            response.StatusCode = (int)httpResponse.StatusCode;

            var codec = requestFeature.Codec;
            if (codec == null)
                return;

            var stream = await httpResponseContent.ReadAsStreamAsync();
            response.Body = codec.Decode(stream);
        }

        #endregion Private Method
    }
}