using System;
using System.Collections.Generic;

namespace Rabbit.Go
{
    public class RequestOptions
    {
        public static RequestOptions Default { get; } = new RequestOptions { Timeout = TimeSpan.FromSeconds(10) };

        public RequestOptions()
        {
            Properties = new Dictionary<object, object>();
        }

        public TimeSpan Timeout { get; set; }

        public IDictionary<object, object> Properties { get; }
    }
}