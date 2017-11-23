using Grpc.Core;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Server.Grpc.Features;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Grpc.Internal
{
    public class GrpcServerRabbitRequest : IRabbitRequest
    {
        private static readonly Func<IFeatureCollection, IGrpcServerRequestFeature> NullRequestFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;
        private IGrpcServerRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, NullRequestFeature);

        public GrpcServerRabbitRequest(GrpcServerRabbitContext context)
        {
            Context = context;

            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        public GrpcServerRabbitContext Context { get; }

        #region Implementation of IRabbitRequest

        public ServiceUrl Url
        {
            get => RequestFeature.ServiceUrl;
            set => RequestFeature.ServiceUrl = value;
        }

        #endregion Implementation of IRabbitRequest

        public object Request
        {
            get => RequestFeature.Request;
            set => RequestFeature.Request = value;
        }

        public ServerCallContext ServerCallContext
        {
            get => RequestFeature.ServerCallContext;
            set => RequestFeature.ServerCallContext = value;
        }

        #region Help Type

        private struct FeatureInterfaces
        {
            public IGrpcServerRequestFeature Request;
        }

        #endregion Help Type
    }

    public class GrpcServerRabbitResponse
    {
        private static readonly Func<IFeatureCollection, IGrpcServerResponseFeature> NullResponseFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;
        private IGrpcServerResponseFeature ResponseFeature => _features.Fetch(ref _features.Cache.Response, NullResponseFeature);

        public GrpcServerRabbitResponse(GrpcServerRabbitContext context)
        {
            Context = context;
            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        public GrpcServerRabbitContext Context { get; }

        public Type ResponseType
        {
            get => ResponseFeature.ResponseType;
            set => ResponseFeature.ResponseType = value;
        }

        public object Response
        {
            get => ResponseFeature.Response;
            set => ResponseFeature.Response = value;
        }

        public Func<Task<object>> ResponseInvoker
        {
            get => ResponseFeature.ResponseInvoker;
            set => ResponseFeature.ResponseInvoker = value;
        }

        #region Help Type

        private struct FeatureInterfaces
        {
            public IGrpcServerResponseFeature Response;
        }

        #endregion Help Type
    }

    public class GrpcServerRabbitContext : IRabbitContext
    {
        public GrpcServerRabbitContext() : this(new FeatureCollection())
        {
            Request = new GrpcServerRabbitRequest(this);
            Response = new GrpcServerRabbitResponse(this);

            Features.Set<IRequestFeature>(new GrpcServerRequestFeature());
            Features.Set<IGrpcServerRequestFeature>(new GrpcServerRequestFeature());
            Features.Set<IGrpcServerResponseFeature>(new GrpcServerResponseFeature());
        }

        public GrpcServerRabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
            Request = new GrpcServerRabbitRequest(this);
            Response = new GrpcServerRabbitResponse(this);
        }

        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();
        private FeatureReferences<FeatureInterfaces> _features;

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        #region Implementation of IRabbitContext

        public IFeatureCollection Features => _features.Collection;

        public IServiceProvider RequestServices
        {
            get => ServiceProvidersFeature.RequestServices;
            set => ServiceProvidersFeature.RequestServices = value;
        }

        IRabbitRequest IRabbitContext.Request => Request;

        #endregion Implementation of IRabbitContext

        public GrpcServerRabbitRequest Request { get; }
        public GrpcServerRabbitResponse Response { get; }

        #region Help Type

        private struct FeatureInterfaces
        {
            public IServiceProvidersFeature ServiceProviders;
        }

        #endregion Help Type
    }
}