using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Http.Features;
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
            var rabbitClientFeature = context.Features.Get<IRabbitClientFeature>();
            var requestOptions = rabbitClientFeature.RequestOptions;

            var httpRequestFeature = context.Features.Get<IHttpRequestFeature>();
            if (httpRequestFeature == null)
                context.Features.Set(httpRequestFeature = new HttpRequestFeature());

            var request = context.Request;
            if (request.Headers.ContainsKey("Content-Type"))
                httpRequestFeature.ContentType = request.Headers.TryGetValue("Content-Type", out var values) ? values.ToString() : "application/json";

            if (context.Items.ContainsKey("HttpMethod"))
                httpRequestFeature.Method = context.Items["HttpMethod"]?.ToString();

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

            var authority = request.Port >= 0 ? $"{request.Host}:{request.Port}" : request.Host;
            var url = $"{request.Scheme}://{authority}{request.Path}";
            if (request.QueryString.Length > 1)
                url += request.QueryString;

            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(url)
            };

            var httpRequestFeature = context.Features.Get<IHttpRequestFeature>();

            httpRequest.Method = GetHttpMethod(httpRequestFeature.Method, HttpMethod.Get);

            if (request.Body != null)
            {
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
                else if (request.Body is HttpContent bodyContent)
                {
                    httpContent = bodyContent;
                }
                else
                {
                    throw new Exception("不支持的Body类型。");
                }
                httpRequest.Content = httpContent;

                var headers = httpContent.Headers;

                headers.ContentType = null;

                foreach (var header in request.Headers)
                {
                    headers.Add(header.Key, header.Value.ToArray());
                }

                headers.ContentType = new MediaTypeHeaderValue(httpRequestFeature.ContentType ?? "application/json");
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

        private static Task SetResponseAsync(IRabbitContext context, HttpResponseMessage httpResponse)
        {
            var response = context.Response;

            response.StatusCode = (int)httpResponse.StatusCode;

            try
            {
                var httpResponseContent = httpResponse.Content;
                foreach (var header in httpResponse.Headers.Concat(httpResponseContent.Headers))
                {
                    response.Headers[header.Key] = new StringValues(header.Value.ToArray());
                }
                response.Body = httpResponse;
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var requestDetailedException = new HttpRequestDetailedException(httpResponse);
                    throw requestDetailedException;
                }
            }
            catch (Exception e)
            {
                var request = context.Request;
                throw ExceptionUtilities.ServiceRequestFailure(request.Host, (int)httpResponse.StatusCode, e);
            }

            return Task.CompletedTask;
        }

        #endregion Private Method
    }
}