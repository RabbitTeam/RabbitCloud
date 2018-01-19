using Rabbit.Go.Abstractions.Filters;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ExceptionFilterAttribute : Attribute, IAsyncExceptionFilter, IExceptionFilter, IOrderedFilter
    {
        #region Implementation of IAsyncExceptionFilter

        public virtual Task OnExceptionAsync(ExceptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            OnException(context);
            return Task.CompletedTask;
        }

        #endregion Implementation of IAsyncExceptionFilter

        #region Implementation of IExceptionFilter

        public virtual void OnException(ExceptionContext context)
        {
        }

        #endregion Implementation of IExceptionFilter

        #region Implementation of IOrderedFilter

        public int Order { get; set; }

        #endregion Implementation of IOrderedFilter
    }
}