using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client
{
    public class DefaultRabbitRequest : RabbitRequest
    {
        private static readonly Func<IFeatureCollection, IRequestFeature> NullRequestFeature = f => null;
        private static readonly Func<IFeatureCollection, IQueryFeature> NewQueryFeature = f => new QueryFeature(f);
        private FeatureReferences<FeatureInterfaces> _features;

        private IRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, NullRequestFeature);
        private IQueryFeature QueryFeature => _features.Fetch(ref _features.Cache.Query, NewQueryFeature);

        public DefaultRabbitRequest(RabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
            _features = new FeatureReferences<FeatureInterfaces>(rabbitContext.Features);
        }

        #region Overrides of RabbitRequest

        public override RabbitContext RabbitContext { get; }

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

        public override IDictionary<string, StringValues> Query
        {
            get => QueryFeature.Query;
            set => QueryFeature.Query = value;
        }

        public override IDictionary<string, StringValues> Headers => RequestFeature.Headers;

        public override Stream Body
        {
            get => RequestFeature.Body;
            set => RequestFeature.Body = value;
        }

        #endregion Overrides of RabbitRequest

        #region Help Type

        private struct FeatureInterfaces
        {
            public IRequestFeature Request;
            public IQueryFeature Query;
        }

        #endregion Help Type
    }
}