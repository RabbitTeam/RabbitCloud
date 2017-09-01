using System;
using System.Net.Http;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestMappingAttribute : Attribute
    {
        public RequestMappingAttribute(string value = null, string method = null)
        {
            Value = value;
            Method = method ?? HttpMethod.Get.Method;
        }

        public string Value { get; set; }
        public string Method { get; set; }
    }
}