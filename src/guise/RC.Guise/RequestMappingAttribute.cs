using Rabbit.Cloud.Client.Http;
using System;
using System.Net.Http;

namespace Rabbit.Cloud.Guise
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestMappingAttribute : Attribute
    {
        public RequestMappingAttribute()
        {
        }

        public RequestMappingAttribute(string value = null, string method = null)
        {
            Value = value;
            Method = HttpMethodExtensions.GetHttpMethod(method, HttpMethod.Get);
        }

        public string Value { get; set; }
        public HttpMethod Method { get; set; }
    }
}