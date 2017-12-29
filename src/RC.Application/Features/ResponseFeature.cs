using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Rabbit.Cloud.Application.Features
{
    public class ResponseFeature : IResponseFeature
    {
        public ResponseFeature()
        {
            Headers = new HeaderDictionary();
        }

        #region Implementation of IResponseFeature

        public int StatusCode { get; set; }
        public IDictionary<string, StringValues> Headers { get; set; }
        public object Body { get; set; }

        #endregion Implementation of IResponseFeature
    }
}