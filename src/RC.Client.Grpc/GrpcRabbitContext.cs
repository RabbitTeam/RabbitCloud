using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using System;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcRabbitContext : IRabbitContext
    {
        #region Constructor

        public GrpcRabbitContext() : this(new FeatureCollection())
        {
            var grpcRequestFeature = new GrpcRequestFeature();
            Features.Set<IRequestFeature>(grpcRequestFeature);
            Features.Set<IGrpcRequestFeature>(grpcRequestFeature);
            Features.Set<IGrpcResponseFeature>(new GrpcResponseFeature());
        }

        public GrpcRabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
            Request = new GrpcRabbitRequest(this);
            Response = new GrpcRabbitResponse(this);
        }

        #endregion Constructor

        #region Field

        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();
        private FeatureReferences<FeatureInterfaces> _features;

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        #endregion Field

        #region Property

        public IFeatureCollection Features => _features.Collection;

        public IServiceProvider RequestServices
        {
            get => ServiceProvidersFeature.RequestServices;
            set => ServiceProvidersFeature.RequestServices = value;
        }

        public GrpcRabbitRequest Request { get; }

        IRabbitRequest IRabbitContext.Request => Request;
        public GrpcRabbitResponse Response { get; }

        #endregion Property

        #region Help Type

        private struct FeatureInterfaces
        {
            public IServiceProvidersFeature ServiceProviders;
        }

        #endregion Help Type
    }
}