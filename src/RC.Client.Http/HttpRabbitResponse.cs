using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRabbitResponse : IRabbitResponse
    {
        private static readonly Func<IFeatureCollection, IResponseFeature> NullResponseFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;

        public HttpRabbitResponse(HttpRabbitContext context)
        {
            RabbitContext = context;
            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        private IResponseFeature ResponseFeature =>
            _features.Fetch(ref _features.Cache.Response, NullResponseFeature);

        #region Overrides of RabbitResponse

        public IRabbitContext RabbitContext { get; }

        public int StatusCode
        {
            get => ResponseFeature.StatusCode;
            set => ResponseFeature.StatusCode = value;
        }

        public IDictionary<string, StringValues> Headers => ResponseFeature.Headers;

        public Stream Body
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