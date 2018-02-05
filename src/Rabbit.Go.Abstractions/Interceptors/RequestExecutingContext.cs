using System.Collections.Generic;

namespace Rabbit.Go.Interceptors
{
    public class RequestExecutingContext : InterceptorContext
    {
        public RequestExecutingContext(RequestContext requestContext, IList<IInterceptorMetadata> interceptors, IDictionary<string, object> arguments)
            : base(requestContext, interceptors)
        {
            Arguments = arguments;
        }

        public object Result { get; set; }
        public IDictionary<string, object> Arguments { get; }
    }
}