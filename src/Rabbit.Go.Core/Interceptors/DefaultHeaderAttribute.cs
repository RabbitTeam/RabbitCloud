using System;

namespace Rabbit.Go.Interceptors
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class DefaultHeaderAttribute : RequestInterceptorAttribute
    {
        public string Name { get; }
        public string Value { get; }

        public DefaultHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #region Overrides of RequestInterceptorAttribute

        public override void OnRequestExecuting(RequestExecutingContext context)
        {
            context.RequestBuilder.Header(Name, Value);
        }

        #endregion Overrides of RequestInterceptorAttribute
    }
}