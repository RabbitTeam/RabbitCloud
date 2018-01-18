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

            var path = requestModel.Path;

            var rabbitContext = requestContext.RabbitContext;

            string queryString = null;
            // build queryString
            if (rabbitContext.Request.Query is GoQueryCollection query && query.Any())
            {
                rabbitContext.Request.Query = query;
                queryString = rabbitContext.Request.QueryString;
            }

            var pathAndQuery = path.PathTemplate;
            if (queryString != null)
                pathAndQuery += queryString;

            // render template
            if (path.Variables != null && path.Variables.Any() && requestContext.PathVariables != null && requestContext.PathVariables.Any())
                pathAndQuery = _templateEngine.Render(pathAndQuery, requestContext.PathVariables.ToDictionary(i => i.Key, i => i.Value.ToString()))?.Result ?? pathAndQuery;

            var request = rabbitContext.Request;

            var requestCache = GetRequestCache(requestModel);

            request.Scheme = requestCache.Scheme;
            request.Host = requestCache.Host;
            request.Port = requestCache.Port;
            request.Path = pathAndQuery;
        }

        private readonly ConcurrentDictionary<RequestModel, RequestCache> _requestCaches = new ConcurrentDictionary<RequestModel, RequestCache>();

        private RequestCache GetRequestCache(RequestModel requestModel)
        {
            if (_requestCaches.TryGetValue(requestModel, out var cache))
                return cache;

            cache = new RequestCache(requestModel.ServiceModel.Url);

            _requestCaches[requestModel] = cache;

            return cache;
        }

        private struct RequestCache
        {
            public RequestCache(string baseUrl)
            {
                var uri = new Uri(baseUrl);

                Host = uri.Host;
                Scheme = uri.Scheme;
                Port = uri.Port;
            }

            public string Host { get; }
            public string Scheme { get; }
            public int Port { get; }
        }
    }
}