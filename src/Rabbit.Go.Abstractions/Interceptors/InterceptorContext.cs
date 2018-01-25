namespace Rabbit.Go.Interceptors
{
    public abstract class InterceptorContext
    {
        protected InterceptorContext(RequestMessageBuilder requestBuilder)
        {
            RequestBuilder = requestBuilder;
        }

        public RequestMessageBuilder RequestBuilder { get; }
        /*        protected InterceptorContext(RequestContext requestContext)
                {
                    Method = requestContext.Method;
                    Body = requestContext.Body;
                    Charset = requestContext.Charset;
                    Headers = requestContext.Headers;
                    Host = requestContext.Host;
                    Path = requestContext.Path;
                    Port = requestContext.Port;
                    Query = requestContext.Query;
                    Scheme = requestContext.Scheme;
                    RequestServices = requestContext.RequestServices;
                    MethodDescriptor = requestContext.MethodDescriptor;
                    Options = requestContext.Options;
                }*/
    }
}