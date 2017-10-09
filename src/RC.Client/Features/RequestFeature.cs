using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace Rabbit.Cloud.Client.Features
{
    public class RequestFeature : IRequestFeature
    {
        #region Implementation of IRequestFeature

        public string ServiceName { get; set; }
        public string Scheme { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public IDictionary<string, StringValues> Headers { get; set; }
        public Stream Body { get; set; }

        #endregion Implementation of IRequestFeature
    }
}