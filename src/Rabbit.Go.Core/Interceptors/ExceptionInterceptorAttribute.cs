using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Interceptors
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class ExceptionInterceptorAttribute : Attribute, IAsyncExceptionInterceptor, IOrderedInterceptor
    {
        #region Implementation of IAsyncExceptionInterceptor

        public Task OnExceptionAsync(ExceptionInterceptorContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            OnException(context);

            return Task.CompletedTask;
        }

        #endregion Implementation of IAsyncExceptionInterceptor

        public virtual void OnException(ExceptionInterceptorContext context)
        {
        }

        #region Implementation of IOrderedInterceptor

        public int Order { get; set; }

        #endregion Implementation of IOrderedInterceptor
    }
}