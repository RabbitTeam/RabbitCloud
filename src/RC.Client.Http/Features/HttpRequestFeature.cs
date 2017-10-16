using Rabbit.Cloud.Client.Features;
using System.Net.Http;

namespace Rabbit.Cloud.Client.Http.Features
{
    public class HttpRequestFeature : RequestFeature, IHttpRequestFeature
    {
        public HttpRequestFeature()
        {
            Method = HttpMethod.Get;
        }

        #region Implementation of IHttpRequestFeature

        public HttpMethod Method { get; set; }

        #endregion Implementation of IHttpRequestFeature
    }
}