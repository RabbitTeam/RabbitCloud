using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Rabbit.Cloud.Client.Features
{
    public class RequestFeature : IRequestFeature
    {
        public RequestFeature()
        {
            Headers = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);
            Body = Stream.Null;
            Method = HttpMethod.Get.Method;
        }

        #region Implementation of IRequestFeature

        public string Method { get; set; }
        public Uri RequestUri { get; set; }
        public IDictionary<string, StringValues> Headers { get; set; }
        public Stream Body { get; set; }

        #endregion Implementation of IRequestFeature
    }
}