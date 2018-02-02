namespace Rabbit.Go
{
    public class RequestContext
    {
        public RequestContext(RequestContext requestContext)
            : this(requestContext.GoContext, requestContext.MethodDescriptor)
        {
        }

        public RequestContext(GoContext goContext, MethodDescriptor methodDescriptor)
        {
            GoContext = goContext;
            MethodDescriptor = methodDescriptor;
        }

        public GoContext GoContext { get; }
        public MethodDescriptor MethodDescriptor { get; set; }
    }
}