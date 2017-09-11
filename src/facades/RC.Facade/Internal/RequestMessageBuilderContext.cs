using Castle.DynamicProxy;
using System.Collections.Generic;
using System.Net.Http;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageBuilderContext
    {
        private readonly IDictionary<string, object> _arguments = new Dictionary<string, object>();

        public RequestMessageBuilderContext(IInvocation invocation, HttpRequestMessage requestMessage)
        {
            Invocation = invocation;
            RequestMessage = requestMessage;

            var index = 0;
            foreach (var parameterInfo in invocation.Method.GetParameters())
            {
                _arguments[parameterInfo.Name] = invocation.Arguments[index];
                index = index + 1;
            }
        }

        public IInvocation Invocation { get; }
        public HttpRequestMessage RequestMessage { get; }
        public bool Canceled { get; set; }

        public object GetArgument(string parameterName)
        {
            _arguments.TryGetValue(parameterName, out var value);
            return value;
        }
    }
}