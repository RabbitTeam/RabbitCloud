using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Interceptors
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class RequestInterceptorAttribute : Attribute, IAsyncRequestInterceptor, IOrderedInterceptor
    {
        #region Implementation of IAsyncRequestInterceptor

        public virtual async Task OnActionExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            OnRequestExecuting(context);

            if (context.Result == null)
                OnRequestExecuted(await next());
        }

        #endregion Implementation of IAsyncRequestInterceptor

        public virtual void OnRequestExecuting(RequestExecutingContext context)
        {
        }

        public virtual void OnRequestExecuted(RequestExecutedContext context)
        {
        }

        #region Implementation of IOrderedInterceptor

        public int Order { get; set; }

        #endregion Implementation of IOrderedInterceptor
    }
}