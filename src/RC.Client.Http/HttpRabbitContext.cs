using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Http.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRabbitContext : IRabbitContext
    {
        private static readonly Func<IFeatureCollection, IItemsFeature> NewItemsFeature = f => new ItemsFeature();
        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();

        private FeatureReferences<FeatureInterfaces> _features;

        public HttpRabbitContext()
            : this(new FeatureCollection())
        {
            Features.Set<IHttpRequestFeature>(new HttpRequestFeature());
            Features.Set<IHttpResponseFeature>(new HttpResponseFeature());
            Features.Set<IRequestFeature>(Features.Get<IHttpRequestFeature>());
            Features.Set<IResponseFeature>(Features.Get<IHttpResponseFeature>());
        }

        public HttpRabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
            Request = InitializeHttpRequest();
            Response = InitializeHttpResponse();
        }

        private HttpRabbitRequest InitializeHttpRequest() => new HttpRabbitRequest(this);

        private HttpRabbitResponse InitializeHttpResponse() => new HttpRabbitResponse(this);

        private IItemsFeature ItemsFeature =>
            _features.Fetch(ref _features.Cache.Items, NewItemsFeature);

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        #region Overrides of RabbitContext

        public IFeatureCollection Features => _features.Collection;
        public HttpRabbitRequest Request { get; }
        IRabbitRequest IRabbitContext.Request => Request;

        IRabbitResponse IRabbitContext.Response => Response;
        public HttpRabbitResponse Response { get; }

        public IDictionary<object, object> Items
        {
            get => ItemsFeature.Items;
            set => ItemsFeature.Items = value;
        }

        public IServiceProvider RequestServices
        {
            get => ServiceProvidersFeature.RequestServices;
            set => ServiceProvidersFeature.RequestServices = value;
        }

        #endregion Overrides of RabbitContext

        #region Help Type

        private struct FeatureInterfaces
        {
            public IItemsFeature Items;
            public IServiceProvidersFeature ServiceProviders;
        }

        #endregion Help Type
    }
}