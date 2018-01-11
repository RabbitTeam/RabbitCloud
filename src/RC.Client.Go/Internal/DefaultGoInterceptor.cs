using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using Rabbit.Cloud.Client.Go.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Client.Go.Internal
{
    public class DefaultGoInterceptor : GoInterceptor
    {
        #region Field

        private readonly RabbitRequestDelegate _invoker;

        private readonly ConcurrentDictionary<(Type, MethodInfo), RequestEntry> _requestEntries = new ConcurrentDictionary<(Type, MethodInfo), RequestEntry>();

        #endregion Field

        #region Constructor

        public DefaultGoInterceptor(RabbitRequestDelegate invoker, ApplicationModel applicationModel, IServiceProvider serviceProvider) : base(applicationModel, serviceProvider)
        {
            _invoker = invoker;
        }

        #endregion Constructor

        #region Overrides of GoInterceptor

        protected override IServiceInvoker CreateServiceInvoker(InterceptContext interceptContext)
        {
            var goRequestContext = CreateGoRequestContext(interceptContext);

            var invoker = new DefaultServiceInvoker(new ServiceInvokerContext
            {
                RequestModel = interceptContext.RequestModel,
                RequestContext = goRequestContext,
                Invoker = _invoker
            });

            return invoker;
        }

        #endregion Overrides of GoInterceptor

        #region Protected Method

        protected virtual GoRequestContext CreateGoRequestContext(InterceptContext interceptContext)
        {
            var invocation = interceptContext.Invocation;

            var entry = GetRequestEntry(interceptContext);
            var goRequestContext = new GoRequestContext(new RabbitContext())
            {
                RequestUrl = entry.Uri,
                Arguments = invocation.MappingArguments()
            };

            goRequestContext.AppendQuery(entry.DefaultQuery);
            goRequestContext.AppendHeaders(entry.DefaultHeaders);
            goRequestContext.AppendItems(entry.DefaultItems);

            return goRequestContext;
        }

        protected virtual RequestEntry CreateRequestEntry(InterceptContext interceptContext)
        {
            var requestModel = interceptContext.RequestModel;
            var serviceModel = requestModel.ServiceModel;

            IEnumerable<IGoRequestOptionsProvider> GetRequestOptions()
            {
                yield return requestModel.Attributes.OfType<IGoRequestOptionsProvider>().FirstOrDefault();
                yield return requestModel.ServiceModel.Attributes.OfType<IGoRequestOptionsProvider>().FirstOrDefault();
                yield return DefaultRequestOptions.RequestOptions;
            }

            var entry = new RequestEntry
            {
                RequestModel = requestModel,
                DefaultHeaders = requestModel.GetRequestHeaders(),
                DefaultItems = requestModel.GetRequestItems()
                //                                RequestOptions = GetRequestOptions().FirstOrDefault(o => o != null)
            };

            var uri = serviceModel.Url.TrimEnd('/') + "/" + requestModel.Path.ToString().TrimStart('/');
            entry.Uri = uri;

            entry.DefaultQuery = QueryHelpers.ParseNullableQuery(uri);

            return entry;
        }

        #endregion Protected Method

        #region Private Method

        private RequestEntry GetRequestEntry(InterceptContext interceptContext)
        {
            var requestModel = interceptContext.RequestModel;
            var proxyType = requestModel.ServiceModel.Type;
            var proxyMethod = requestModel.MethodInfo;
            return _requestEntries.GetOrAdd((proxyType, proxyMethod), key => CreateRequestEntry(interceptContext));
        }

        #endregion Private Method

        #region Help Type

        protected struct RequestEntry
        {
            public RequestModel RequestModel { get; set; }
            public string Uri { get; set; }
            public IDictionary<object, object> DefaultItems { get; set; }
            public IDictionary<string, StringValues> DefaultHeaders { get; set; }
            public IDictionary<string, StringValues> DefaultQuery { get; set; }
        }

        private class DefaultRequestOptions : IGoRequestOptionsProvider
        {
            public static IGoRequestOptionsProvider RequestOptions { get; } = new DefaultRequestOptions();
            public bool NotFoundReturnNull { get; } = true;
        }

        #endregion Help Type
    }
}