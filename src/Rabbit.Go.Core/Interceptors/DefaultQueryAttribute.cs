using System;

namespace Rabbit.Go.Interceptors
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class DefaultQueryAttribute : RequestInterceptorAttribute
    {
        public string Name { get; }
        public string Value { get; }

        public DefaultQueryAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #region Overrides of RequestInterceptorAttribute

        public override void OnRequestExecuting(RequestExecutingContext context)
        {
            context.RequestBuilder.AddQuery(Name, Value);
        }

        #endregion Overrides of RequestInterceptorAttribute
    }
}