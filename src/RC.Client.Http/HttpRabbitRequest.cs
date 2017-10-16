using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Http.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Rabbit.Cloud.Client.Http
{
    public class HttpRabbitRequest : IRabbitRequest
    {
        private static readonly Func<IFeatureCollection, IHttpRequestFeature> NullRequestFeature = f => null;
        private FeatureReferences<FeatureInterfaces> _features;

        private IHttpRequestFeature RequestFeature => _features.Fetch(ref _features.Cache.Request, NullRequestFeature);

        public HttpRabbitRequest(HttpRabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;

            _features = new FeatureReferences<FeatureInterfaces>(rabbitContext.Features);
        }

        #region Overrides of RabbitRequest

        IRabbitContext IRabbitRequest.RabbitContext => RabbitContext;
        public HttpRabbitContext RabbitContext { get; }

        public HttpMethod Method
        {
            get => RequestFeature.Method;
            set => RequestFeature.Method = value;
        }

        public Uri RequestUri
        {
            get => RequestFeature.RequestUri;
            set => RequestFeature.RequestUri = value;
        }

        public IDictionary<string, StringValues> Headers => RequestFeature.Headers;

        public Stream Body
        {
            get => RequestFeature.Body;
            set => RequestFeature.Body = value;
        }

        #endregion Overrides of RabbitRequest

        #region Help Type

        private struct FeatureInterfaces
        {
            public IHttpRequestFeature Request;
        }

        #endregion Help Type
    }
}