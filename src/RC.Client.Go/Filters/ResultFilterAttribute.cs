using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ResultFilterAttribute : Attribute, IResultFilter, IAsyncResultFilter, IOrderedFilter
    {
        #region Implementation of IResultFilter

        public virtual void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public virtual void OnResultExecuting(ResultExecutingContext context)
        {
        }

        #endregion Implementation of IResultFilter

        #region Implementation of IAsyncResultFilter

        public virtual async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            OnResultExecuting(context);
            if (!context.Cancel)
            {
                OnResultExecuted(await next());
            }
        }

        #endregion Implementation of IAsyncResultFilter

        #region Implementation of IOrderedFilter

        public int Order { get; set; }

        #endregion Implementation of IOrderedFilter
    }
}