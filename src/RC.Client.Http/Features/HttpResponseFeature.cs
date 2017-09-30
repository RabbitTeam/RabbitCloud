using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Features;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Http.Features
{
    public interface IHttpResponseFeature : IResponseFeature
    {
        IDictionary<string, StringValues> Headers { get; set; }
    }

    public class HttpResponseFeature : ResponseFeature, IHttpResponseFeature
    {
        #region Implementation of IHttpResponseFeature

        public IDictionary<string, StringValues> Headers { get; set; }

        #endregion Implementation of IHttpResponseFeature
    }
}