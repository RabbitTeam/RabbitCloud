namespace Rabbit.Go.Interceptors
{
    public class RequestExecutingContext : InterceptorContext
    {
        public RequestExecutingContext(RequestMessageBuilder requestBuilder) : base(requestBuilder)
        {
        }

        public object Result { get; set; }
    }
}