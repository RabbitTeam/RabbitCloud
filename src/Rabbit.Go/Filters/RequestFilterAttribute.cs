using Rabbit.Go.Abstractions.Filters;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Filters
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class RequestFilterAttribute : Attribute, IRequestFilter, IAsyncRequestFilter, IOrderedFilter
    {
        #region Implementation of IRequestFilter

        public virtual void OnRequestExecuting(RequestExecutingContext context)
        {
        }

        public virtual void OnRequestExecuted(RequestExecutedContext context)
        {
        }

        #endregion Implementation of IRequestFilter

        #region Implementation of IAsyncRequestFilter

        public virtual async Task OnRequestExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (next == null)
                throw new ArgumentNullException(nameof(next));

            OnRequestExecuting(context);
            if (context.Result == null)
            {
                OnRequestExecuted(await next());
            }
        }

        #endregion Implementation of IAsyncRequestFilter

        #region Implementation of IOrderedFilter

        public int Order { get; set; }

        #endregion Implementation of IOrderedFilter
    }
}