using Rabbit.Go.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Go.Interceptors
{
    public abstract class InterceptorContext : RequestContext
    {
        protected InterceptorContext(RequestContext requestContext, IList<IInterceptorMetadata> interceptors)
            : base(requestContext)
        {
            Interceptors = interceptors;
        }

        public IList<IInterceptorMetadata> Interceptors { get; }
    }
}