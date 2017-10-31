using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;

namespace Rabbit.Cloud.Client.Grpc
{
    public class ServiceProvidersFeature : IServiceProvidersFeature
    {
        public IServiceProvider RequestServices { get; set; }
    }

    public class GrpcRabbitContext : IRabbitContext
    {
        public GrpcRabbitContext() : this(new FeatureCollection())
        {
        }

        public GrpcRabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
        }

        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();
        private FeatureReferences<FeatureInterfaces> _features;

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        public IFeatureCollection Features => _features.Collection;

        public IServiceProvider RequestServices
        {
            get => ServiceProvidersFeature.RequestServices;
            set => ServiceProvidersFeature.RequestServices = value;
        }

        private struct FeatureInterfaces
        {
            public IItemsFeature Items;
            public IServiceProvidersFeature ServiceProviders;
        }
    }
}