using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client
{
    public class DefaultRabbitContext : RabbitContext
    {
        private static readonly Func<IFeatureCollection, IItemsFeature> NewItemsFeature = f => new ItemsFeature();
        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();

        private FeatureReferences<FeatureInterfaces> _features;

        public DefaultRabbitContext()
            : this(new FeatureCollection())
        {
            Features.Set<IRequestFeature>(new RequestFeature());
            Features.Set<IResponseFeature>(new ResponseFeature());
        }

        public DefaultRabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
            Request = InitializeHttpRequest();
            Response = InitializeHttpResponse();
        }

        private RabbitRequest InitializeHttpRequest() => new DefaultRabbitRequest(this);

        private RabbitResponse InitializeHttpResponse() => new DefaultRabbitResponse(this);

        private IItemsFeature ItemsFeature =>
            _features.Fetch(ref _features.Cache.Items, NewItemsFeature);

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        #region Overrides of RabbitContext

        public override IFeatureCollection Features => _features.Collection;
        public override RabbitRequest Request { get; }

        public override RabbitResponse Response { get; }

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