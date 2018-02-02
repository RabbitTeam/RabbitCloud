using System.Collections.Generic;

namespace Rabbit.Go.Interceptors
{
    public class RequestExecutingContext : InterceptorContext
    {
        public RequestExecutingContext(RequestContext requestContext, IList<IInterceptorMetadata> interceptors)
            : base(requestContext, interceptors)
        {
        }

        public object Result { get; set; }
    }
}