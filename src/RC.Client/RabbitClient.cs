using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class RabbitRequestMessage
    {
        public RabbitRequestMessage(Type requesType, Type responseType, Uri url, object body = null, IDictionary<string, StringValues> headers = null, IDictionary<object, object> items = null)
        {
            Scheme = url.Scheme;
            Host = url.Host;
            Path = url.PathAndQuery.Substring(0, url.PathAndQuery.Length - url.Query.Length);
            Port = url.IsDefaultPort ? -1 : url.Port;
            QueryString = url.Query;
            RequesType = requesType;
            ResponseType = responseType;
            Body = body;
            Headers = headers != null ? new Dictionary<string, StringValues>(headers, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            Items = items != null ? new Dictionary<object, object>(items) : new Dictionary<object, object>();
        }

        public RabbitRequestMessage()
        {
        }

        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public int Port { get; set; }
        public string QueryString { get; set; }
        public IDictionary<string, StringValues> Headers { get; }
        public IDictionary<object, object> Items { get; }
        public object Body { get; set; }

        public Type RequesType { get; set; }
        public Type ResponseType { get; set; }
    }

    public class RabbitResponseMessage
    {
        public RabbitResponseMessage(IDictionary<string, StringValues> headers)
        {
            Headers = headers != null ? new Dictionary<string, StringValues>(headers, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
        }

        public int StatusCode { get; set; }
        public object Body { get; set; }
        public IDictionary<string, StringValues> Headers { get; }
    }

    public interface IRabbitClient
    {
        Task<RabbitResponseMessage> SendAsync(RabbitRequestMessage request);
    }

    public class RabbitClient : IRabbitClient
    {
        private readonly RabbitRequestDelegate _requestDelegate;
        private readonly IServiceProvider _serviceProvider;

        public RabbitClient(RabbitRequestDelegate requestDelegate, IServiceProvider serviceProvider)
        {
            _requestDelegate = requestDelegate;
            _serviceProvider = serviceProvider;
        }

        #region Implementation of IRabbitClient

        public async Task<RabbitResponseMessage> SendAsync(RabbitRequestMessage request)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var rabbitContext = new RabbitContext
                {
                    RequestServices = scope.ServiceProvider
                };
                var rabbitRequest = rabbitContext.Request;
                rabbitRequest.Scheme = request.Scheme;
                rabbitRequest.Host = request.Host;
                rabbitRequest.Path = request.Path;
                rabbitRequest.Port = request.Port;
                rabbitRequest.QueryString = request.QueryString;
                rabbitRequest.Body = request.Body;
                rabbitRequest.Headers = request.Headers;
                rabbitContext.Items = request.Items;

                rabbitContext.Features.Set<IServiceRequestFeature>(new ServiceRequestFeature(rabbitRequest)
                {
                    RequesType = request.RequesType,
                    ResponseType = request.ResponseType
                });

                await _requestDelegate(rabbitContext);

                var rabbitResponse = rabbitContext.Response;
                return new RabbitResponseMessage(rabbitResponse.Headers)
                {
                    Body = rabbitResponse.Body,
                    StatusCode = rabbitResponse.StatusCode
                };
            }
        }

        #endregion Implementation of IRabbitClient
    }

    public static class RabbitClientExtensions
    {
        public static async Task<TResponse> SendAsync<TRequest, TResponse>(this IRabbitClient rabbitClient, string url, TRequest request, IDictionary<string, StringValues> headers = null)
        {
            var response = await rabbitClient.SendAsync(new RabbitRequestMessage(typeof(TRequest), typeof(TResponse), new Uri(url), request, headers));
            var body = response.Body;
            if (body is Task<TResponse> task)
                return await task;
            return (TResponse)response.Body;
        }
    }
}