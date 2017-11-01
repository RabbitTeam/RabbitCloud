using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using System;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcRabbitResponse
    {
        #region Field

        private static readonly Func<IFeatureCollection, IGrpcResponseFeature> NullResponseFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;
        private IGrpcResponseFeature ResponseFeature => _features.Fetch(ref _features.Cache.Response, NullResponseFeature);

        #endregion Field

        #region Constructor

        public GrpcRabbitResponse(GrpcRabbitContext grpcRabbitContext)
        {
            RabbitContext = grpcRabbitContext;

            _features = new FeatureReferences<FeatureInterfaces>(grpcRabbitContext.Features);
        }

        #endregion Constructor

        #region Property

        public GrpcRabbitContext RabbitContext { get; }

        public object Response
        {
            get => ResponseFeature.Response;
            set => ResponseFeature.Response = value;
        }

        #endregion Property

        #region Help Type

        private struct FeatureInterfaces
        {
            public IGrpcResponseFeature Response;
        }

        #endregion Help Type
    }
}