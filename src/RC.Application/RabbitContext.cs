using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application
{
    public class RabbitRequest : IRabbitRequest
    {
        private static readonly Func<IFeatureCollection, IRequestFeature> NullRequestFeature = f => null;
        private static readonly Func<IFeatureCollection, IQueryFeature> NewQueryFeature = f => new QueryFeature(f);

        private FeatureReferences<FeatureInterfaces> _features;

        public RabbitRequest(IRabbitContext context)
        {
            RabbitContext = context;
            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        private IRequestFeature RequestFeature =>
            _features.Fetch(ref _features.Cache.Request, NullRequestFeature);

        private IQueryFeature QueryFeature =>
            _features.Fetch(ref _features.Cache.Query, NewQueryFeature);

        #region Implementation of IRabbitRequest

        public IRabbitContext RabbitContext { get; }

        public string Scheme
        {
            get => RequestFeature.Scheme;
            set => RequestFeature.Scheme = value;
        }

        public string Host
        {
            get => RequestFeature.Host;
            set => RequestFeature.Host = value;
        }

        public int Port
        {
            get => RequestFeature.Port;
            set => RequestFeature.Port = value;
        }

        public string Path
        {
            get => RequestFeature.Path;
            set => RequestFeature.Path = value;
        }

        public string QueryString
        {
            get => RequestFeature.QueryString;
            set => RequestFeature.QueryString = value;
        }

        public IDictionary<string, StringValues> Query
        {
            get => QueryFeature.Query;
            set => QueryFeature.Query = value;
        }

        public IDictionary<string, StringValues> Headers
        {
            get => RequestFeature.Headers;
            set => RequestFeature.Headers = value;
        }

        public object Body
        {
            get => RequestFeature.Body;
            set => RequestFeature.Body = value;
        }

        #endregion Implementation of IRabbitRequest

        private struct FeatureInterfaces
        {
            public IRequestFeature Request;
            public IQueryFeature Query;
        }
    }

    public class RabbitResponse : IRabbitResponse
    {
        private static readonly Func<IFeatureCollection, IResponseFeature> NullResponseFeature = f => null;

        private FeatureReferences<FeatureInterfaces> _features;

        public RabbitResponse(IRabbitContext context)
        {
            RabbitContext = context;
            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        private IResponseFeature ResponseFeature =>
            _features.Fetch(ref _features.Cache.Response, NullResponseFeature);

        #region Implementation of IRabbitResponse

        public IRabbitContext RabbitContext { get; }

        public int StatusCode
        {
            get => ResponseFeature.StatusCode;
            set => ResponseFeature.StatusCode = value;
        }

        public IDictionary<string, StringValues> Headers
        {
            get => ResponseFeature.Headers;
            set => ResponseFeature.Headers = value;
        }

        public object Body
        {
            get => ResponseFeature.Body;
            set => ResponseFeature.Body = value;
        }

        #endregion Implementation of IRabbitResponse

        private struct FeatureInterfaces
        {
            public IResponseFeature Response;
        }
    }

    public class RabbitContext : IRabbitContext
    {
        private static readonly Func<IFeatureCollection, IItemsFeature> NewItemsFeature = f => new ItemsFeature();
        private static readonly Func<IFeatureCollection, IServiceProvidersFeature> NewServiceProvidersFeature = f => new ServiceProvidersFeature();
        private FeatureReferences<FeatureInterfaces> _features;

        private IServiceProvidersFeature ServiceProvidersFeature =>
            _features.Fetch(ref _features.Cache.ServiceProviders, NewServiceProvidersFeature);

        private IItemsFeature ItemsFeature =>
            _features.Fetch(ref _features.Cache.Items, NewItemsFeature);

        #region Constructor

        public RabbitContext() : this(new FeatureCollection())
        {
            Features.Set<IRequestFeature>(new RequestFeature());
            Features.Set<IResponseFeature>(new ResponseFeature());
        }

        public RabbitContext(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
            Request = new RabbitRequest(this);
            Response = new RabbitResponse(this);
        }

        #endregion Constructor

        #region Implementation of IRabbitContext

        public IFeatureCollection Features => _features.Collection;

        public IServiceProvider RequestServices
        {
            get => ServiceProvidersFeature.RequestServices;
            set => ServiceProvidersFeature.RequestServices = value;
        }

        public IRabbitRequest Request { get; }
        public IRabbitResponse Response { get; }

        public IDictionary<object, object> Items
        {
            get => ItemsFeature.Items;
            set => ItemsFeature.Items = value;
        }

        #endregion Implementation of IRabbitContext

        private struct FeatureInterfaces
        {
            public IItemsFeature Items;
            public IServiceProvidersFeature ServiceProviders;
        }
    }
}