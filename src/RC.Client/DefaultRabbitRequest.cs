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
        private FeatureReferences<FeatureInterfaces> _features;

        private IRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, NullRequestFeature);

        public DefaultRabbitRequest(RabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
            _features = new FeatureReferences<FeatureInterfaces>(rabbitContext.Features);
        }

        #region Overrides of RabbitRequest

        public override RabbitContext RabbitContext { get; }

        public override string Method
        {
            get => RequestFeature.Method;
            set => RequestFeature.Method = value;
        }

        public override Uri RequestUri
        {
            get => RequestFeature.RequestUri;
            set => RequestFeature.RequestUri = value;
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
        }

        #endregion Help Type
    }
}