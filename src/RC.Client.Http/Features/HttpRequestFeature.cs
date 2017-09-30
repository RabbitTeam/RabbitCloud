using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Features;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Http.Features
{
    public interface IHttpRequestFeature : IRequestFeature
    {
        IDictionary<string, StringValues> Headers { get; set; }
    }

    public class HttpRequestFeature : RequestFeature, IHttpRequestFeature
    {
        #region Implementation of IHttpRequestFeature

        public IDictionary<string, StringValues> Headers { get; set; }

        #endregion Implementation of IHttpRequestFeature
    }
}