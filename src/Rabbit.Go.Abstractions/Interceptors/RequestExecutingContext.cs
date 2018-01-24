namespace Rabbit.Go.Interceptors
{
    public class RequestExecutingContext : InterceptorContext
    {
        public RequestExecutingContext(RequestContext requestContext) : base(requestContext)
        {
        }

        public object Result { get; set; }
    }
}