using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Http.Features;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRabbitRequest : RabbitRequest<HttpRabbitContext>
    {
        private static readonly Func<IFeatureCollection, IHttpRequestFeature> NullRequestFeature = f => null;
        private static readonly Func<IFeatureCollection, IQueryFeature> NewQueryFeature = f => new QueryFeature(f);
        private FeatureReferences<FeatureInterfaces> _features;

        private IHttpRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, NullRequestFeature);
        private IQueryFeature QueryFeature => _features.Fetch(ref _features.Cache.Query, NewQueryFeature);

        public HttpRabbitRequest(HttpRabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
            _features = new FeatureReferences<FeatureInterfaces>(rabbitContext.Features);
        }

        #region Overrides of RabbitRequest

        public override HttpRabbitContext RabbitContext { get; }

        public override string ServiceName
        {
            get => RequestFeature.ServiceName;
            set => RequestFeature.ServiceName = value;
        }

        public override string Scheme
        {
            get => RequestFeature.Scheme;
            set => RequestFeature.Scheme = value;
        }

        public override string Path
        {
            get => RequestFeature.Path;
            set => RequestFeature.Path = value;
        }

        public override string QueryString
        {
            get => RequestFeature.QueryString;
            set => RequestFeature.QueryString = value;
        }

        public override Stream Body
        {
            get => RequestFeature.Body;
            set => RequestFeature.Body = value;
        }

        public override IDictionary<string, StringValues> Query
        {
            get => QueryFeature.Query;
            set => QueryFeature.Query = value;
        }

        #endregion Overrides of RabbitRequest

        public IDictionary<string, StringValues> Headers => RequestFeature.Headers;

        #region Help Type

        private struct FeatureInterfaces
        {
            public IHttpRequestFeature Request;
            public IQueryFeature Query;
        }

        #endregion Help Type
    }
}