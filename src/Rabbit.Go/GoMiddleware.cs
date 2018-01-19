using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Go.Abstractions;
using Rabbit.Go.ApplicationModels;
using Rabbit.Go.Binder;
using Rabbit.Go.Features;
using Rabbit.Go.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Rabbit.Go
{
    public class GoMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly TemplateEngine _templateEngine = new TemplateEngine();

        public GoMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var goFeature = context.Features.Get<IGoFeature>();
            IRabbitClientFeature rabbitClientFeature = new RabbitClientFeature
            {
                RequestType = goFeature.RequestModel.RequesType,
                ResponseType = goFeature.RequestModel.ResponseType
            };

            context.Features.Set(rabbitClientFeature);
            await BindRabbitContextAsync(goFeature);
            await _next(context);
        }

        private async Task BindRabbitContextAsync(IGoFeature goFeature)
        {
            var requestContext = goFeature.RequestContext;
            var requestModel = goFeature.RequestModel;

            var requestCache = GetRequestCache(requestModel);
            var rabbitContext = requestContext.RabbitContext;

            var request = rabbitContext.Request;
            request.Scheme = requestCache.Scheme;
            request.Host = requestCache.Host;
            request.Port = requestCache.Port;
            request.Path = requestCache.Path;

            foreach (var parameter in requestModel.Parameters)
            {
                var bindContext = new ParameterBindContext
                {
                    Model = requestContext.Arguments[parameter.ParameterInfo.Name],
                    ModelName = parameter.ParameterName,
                    RequestContext = requestContext,
                    Target = parameter.Target,
                    Type = parameter.ParameterInfo.ParameterType
                };
                var parameterBinder = parameter.Attributes.OfType<IParameterBinder>().FirstOrDefault() ?? DefaultParameterBinder.Instance;
                await parameterBinder.BindAsync(bindContext);
            }

            var pathVariables = requestModel.Path.Variables;

            if (pathVariables != null && pathVariables.Any() && requestContext.PathVariables != null && requestContext.PathVariables.Any())
                request.Path = _templateEngine.Render(request.Path, requestContext.PathVariables.ToDictionary(i => i.Key, i => i.Value.ToString()))?.Result ?? request.Path;

            // build queryString
            if (request.Query is GoQueryCollection goQuery && goQuery.Any())
                request.Query = goQuery;

            var baseQuery = requestCache.Query;

            var currentQuery = request.QueryString;

            if (baseQuery != "?")
            {
                if (currentQuery == "?")
                {
                    currentQuery = baseQuery;
                }
                else
                {
                    currentQuery = baseQuery + "&" + currentQuery.Substring(1);
                }
            }

            request.QueryString = currentQuery;
        }

        private readonly ConcurrentDictionary<RequestModel, RequestCache> _requestCaches = new ConcurrentDictionary<RequestModel, RequestCache>();

        private RequestCache GetRequestCache(RequestModel requestModel)
        {
            if (_requestCaches.TryGetValue(requestModel, out var cache))
                return cache;

            cache = new RequestCache(requestModel);

            _requestCaches[requestModel] = cache;

            return cache;
        }

        private struct RequestCache
        {
            public RequestCache(RequestModel requestModel)
            {
                var baseUrl = requestModel.ServiceModel.Url;
                var path = requestModel.Path.PathTemplate;

                if (baseUrl.EndsWith("/") && path.StartsWith("/"))
                    baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
                else if (!baseUrl.EndsWith("/") && !path.StartsWith("/"))
                    path = "/" + path;

                var uri = new Uri(baseUrl + path);

                Scheme = uri.Scheme;
                Host = uri.Host;
                Port = uri.Port;

                var pathAndQuery = HttpUtility.UrlDecode(uri.PathAndQuery);

                var queryStartIndex = pathAndQuery.IndexOf('?');

                if (queryStartIndex != -1)
                {
                    Path = pathAndQuery.Substring(0, queryStartIndex);
                    Query = pathAndQuery.Substring(queryStartIndex);
                }
                else
                {
                    Path = pathAndQuery;
                    Query = "?";
                }
            }

            public string Host { get; }
            public string Scheme { get; }
            public int Port { get; }
            public string Path { get; }
            public string Query { get; }
        }
    }
}