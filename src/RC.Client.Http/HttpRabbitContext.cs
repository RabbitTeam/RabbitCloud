using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRabbitContext : RabbitContext<HttpRabbitRequest, HttpRabbitResponse>
    {
        private static readonly Func<IFeatureCollection, IItemsFeature> NewItemsFeature = f => new ItemsFeature();
        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();

        private FeatureReferences<FeatureInterfaces> _features;
        private readonly HttpRabbitRequest _rabbitRequest;
        private readonly HttpRabbitResponse _rabbitResponse;

        public HttpRabbitContext()
            : this(new FeatureCollection())
        {
            Features.Set<IRequestFeature>(new RequestFeature());
            Features.Set<IResponseFeature>(new ResponseFeature());
        }

        public HttpRabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
            _rabbitRequest = InitializeHttpRequest();
            _rabbitResponse = InitializeHttpResponse();
        }

        private HttpRabbitRequest InitializeHttpRequest() => new HttpRabbitRequest(this);

        private HttpRabbitResponse InitializeHttpResponse() => new HttpRabbitResponse(this);

        private IItemsFeature ItemsFeature =>
            _features.Fetch(ref _features.Cache.Items, NewItemsFeature);

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        #region Overrides of RabbitContext

        public override IFeatureCollection Features => _features.Collection;
        public override HttpRabbitRequest Request => _rabbitRequest;

        public override HttpRabbitResponse Response => _rabbitResponse;

        public override IDictionary<object, object> Items
        {
            get => ItemsFeature.Items;
            set => ItemsFeature.Items = value;
        }

        public override IServiceProvider RequestServices
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