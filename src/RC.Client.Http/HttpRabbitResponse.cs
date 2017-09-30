using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Http.Features;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRabbitResponse : RabbitResponse<HttpRabbitContext>
    {
        private static readonly Func<IFeatureCollection, IHttpResponseFeature> NullResponseFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;

        public HttpRabbitResponse(HttpRabbitContext context)
        {
            RabbitContext = context;
            _features = new FeatureReferences<FeatureInterfaces>(context.Features);
        }

        private IHttpResponseFeature ResponseFeature =>
            _features.Fetch(ref _features.Cache.Response, NullResponseFeature);

        #region Overrides of RabbitResponse

        public override HttpRabbitContext RabbitContext { get; }

        public override int StatusCode
        {
            get => ResponseFeature.StatusCode;
            set => ResponseFeature.StatusCode = value;
        }

        public override Stream Body
        {
            get => ResponseFeature.Body;
            set => ResponseFeature.Body = value;
        }

        public IDictionary<string, StringValues> Headers => ResponseFeature.Headers;

        #endregion Overrides of RabbitResponse

        private struct FeatureInterfaces
        {
            public IHttpResponseFeature Response;
        }
    }
}