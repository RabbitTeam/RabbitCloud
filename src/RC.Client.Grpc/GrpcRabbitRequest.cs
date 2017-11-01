using Grpc.Core;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using System;
using System.Threading;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcRabbitRequest : IRabbitRequest
    {
        #region Field

        private static readonly Func<IFeatureCollection, IGrpcRequestFeature> NullRequestFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;
        private IGrpcRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, NullRequestFeature);

        #endregion Field

        #region Constructor

        public GrpcRabbitRequest(GrpcRabbitContext grpcRabbitContext)
        {
            RabbitContext = grpcRabbitContext;

            _features = new FeatureReferences<FeatureInterfaces>(grpcRabbitContext.Features);
        }

        #endregion Constructor

        #region Property

        public GrpcRabbitContext RabbitContext { get; }

        public object Request
        {
            get => RequestFeature.Request;
            set => RequestFeature.Request = value;
        }

        public string Host
        {
            get => RequestFeature.Host;
            set => RequestFeature.Host = value;
        }

        public Metadata Headers
        {
            get => RequestFeature.Headers;
            set => RequestFeature.Headers = value;
        }

        public DateTime? Deadline
        {
            get => RequestFeature.Deadline;
            set => RequestFeature.Deadline = value;
        }

        public CallOptions CallOptions
        {
            get => RequestFeature.CallOptions;
            set => RequestFeature.CallOptions = value;
        }

        public CancellationToken CancellationToken
        {
            get => RequestFeature.CancellationToken;
            set => RequestFeature.CancellationToken = value;
        }

        #endregion Property

        #region Implementation of IRabbitRequest

        public ServiceUrl Url
        {
            get => RequestFeature.ServiceUrl;
            set => RequestFeature.ServiceUrl = value;
        }

        #endregion Implementation of IRabbitRequest

        #region Help Type

        private struct FeatureInterfaces
        {
            public IGrpcRequestFeature Request;
        }

        #endregion Help Type
    }
}