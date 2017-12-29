using System;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RequestInvokerProviderContext
    {
        public RequestInvokerProviderContext(RequestContext requestContext)
        {
            RequestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public RequestContext RequestContext { get; }

        public IRequestInvoker Result { get; set; }
    }
}