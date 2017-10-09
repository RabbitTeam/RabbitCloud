using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client
{
    public class DefaultRabbitResponse : RabbitResponse
    {
        private static readonly Func<IFeatureCollection, IResponseFeature> NullResponseFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;

        public DefaultRabbitResponse(RabbitContext context)
        {
            RabbitContext = context;
            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        private IResponseFeature ResponseFeature =>
            _features.Fetch(ref _features.Cache.Response, NullResponseFeature);

        #region Overrides of RabbitResponse

        public override RabbitContext RabbitContext { get; }

        public override int StatusCode
        {
            get => ResponseFeature.StatusCode;
            set => ResponseFeature.StatusCode = value;
        }

        public override IDictionary<string, StringValues> Headers => ResponseFeature.Headers;

        public override Stream Body
        {
            get => ResponseFeature.Body;
            set => ResponseFeature.Body = value;
        }

        #endregion Overrides of RabbitResponse

        private struct FeatureInterfaces
        {
            public IResponseFeature Response;
        }
    }
}