using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using Rabbit.Cloud.Client.Go.Filters;
using Rabbit.Cloud.Client.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Internal
{
    public abstract class ServiceInvoker : IServiceInvoker
    {
        public abstract ServiceInvokerContext InvokerContext { get; }

        #region Implementation of IServiceInvoker

        public abstract Task InvokeAsync();

        #endregion Implementation of IServiceInvoker
    }

    public class DefaultServiceInvoker : ServiceInvoker
    {
        private readonly ITemplateEngine _templateEngine;

        public DefaultServiceInvoker(ServiceInvokerContext invokerContext)
        {
            InvokerContext = invokerContext;
            _templateEngine = new TemplateEngine();
        }

        #region Implementation of IServiceInvoker

        public override ServiceInvokerContext InvokerContext { get; }

        public override async Task InvokeAsync()
        {
            var requestContext = InvokerContext.RequestContext;
            var requestModel = InvokerContext.RequestModel;
            var rabbitContext = requestContext.RabbitContext;

            BindRabbitContext();

            var filters = requestModel
                .GetRequestAttributes()
                .OfType<IFilterMetadata>()
                .OrderBy(f =>
                {
                    if (f is IOrderedFilter orderedFilter)
                        return orderedFilter.Order;

                    return 20;
                }).ToArray();

            var asyncRequestFilters = filters.OfType<IAsyncRequestFilter>().ToList();

            // sync filter convert to async filter
            var requestFilters = filters.OfType<IRequestFilter>()
                .Where(i => !(i is IAsyncRequestFilter))
                .ToArray();
            if (requestFilters.Any())
                asyncRequestFilters.Add(new SyncRequestFilter(requestFilters));

            var executingContext = CreateExecutingContext(filters);
            var executedContext = CreateExecutedContext(filters);

            var isInvokeEnd = false;
            async Task<RequestExecutedContext> InvokeEndAsync()
            {
                try
                {
                    isInvokeEnd = true;
                    executedContext.Result = executingContext.Result;

                    await InvokerContext.Invoker(rabbitContext);
                    executedContext.Result = rabbitContext.Response.Body;

                    return executedContext;
                }
                catch (Exception exception)
                {
                    return executedContext = new RequestExecutedContext(requestContext, filters, requestContext.Arguments)
                    {
                        ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                    };
                }
            }

            if (asyncRequestFilters.Any())
            {
                foreach (var filter in asyncRequestFilters)
                {
                    await filter.OnRequestExecutionAsync(executingContext, InvokeEndAsync);
                }
            }
            else
            {
                await InvokeEndAsync();
            }

            if (!isInvokeEnd)
                executedContext.Result = executingContext.Result;

            if (!executedContext.ExceptionHandled)
                executedContext.ExceptionDispatchInfo?.Throw();

            rabbitContext.Response.Body = executedContext.Result;
        }

        #endregion Implementation of IServiceInvoker

        #region Protected Method

        protected virtual void BindRabbitContext()
        {
            var requestContext = InvokerContext.RequestContext;
            var rabbitContext = requestContext.RabbitContext;
            var requestModel = InvokerContext.RequestModel;

            ParameterUtil.BuildParameters(requestContext, requestModel);

            var url = _templateEngine.Render(requestContext.RequestUrl, requestContext.PathVariables)?.Result ?? requestContext.RequestUrl;

            var request = rabbitContext.Request;

            var uri = new Uri(url);

            request.Host = uri.Host;
            request.Scheme = uri.Scheme;
            request.Port = uri.Port;
            request.Path = uri.PathAndQuery;

            rabbitContext.Features.Set<IServiceRequestFeature>(new ServiceRequestFeature(rabbitContext.Request)
            {
                RequesType = requestModel.RequesType,
                ResponseType = requestModel.ResponseType
            });
        }

        #endregion Protected Method

        #region Private Method

        private RequestExecutingContext CreateExecutingContext(IEnumerable<IFilterMetadata> filters)
        {
            return new RequestExecutingContext(InvokerContext.RequestContext, new List<IFilterMetadata>(filters), InvokerContext.RequestContext.Arguments);
        }

        private RequestExecutedContext CreateExecutedContext(IEnumerable<IFilterMetadata> filters)
        {
            return new RequestExecutedContext(InvokerContext.RequestContext, new List<IFilterMetadata>(filters), InvokerContext.RequestContext.Arguments);
        }

        #endregion Private Method

        #region Help Type

        private class SyncRequestFilter : RequestFilterAttribute
        {
            private readonly IReadOnlyList<IRequestFilter> _filters;

            public SyncRequestFilter(IReadOnlyList<IRequestFilter> filters)
            {
                _filters = filters;
            }

            #region Overrides of RequestFilterAttribute

            public override void OnRequestExecuting(RequestExecutingContext context)
            {
                foreach (var filter in _filters)
                    filter.OnRequestExecuting(context);
            }

            public override void OnRequestExecuted(RequestExecutedContext context)
            {
                foreach (var filter in _filters)
                    filter.OnRequestExecuted(context);
            }

            #endregion Overrides of RequestFilterAttribute
        }

        #endregion Help Type
    }
}