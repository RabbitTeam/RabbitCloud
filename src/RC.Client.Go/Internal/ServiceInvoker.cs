using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using Rabbit.Cloud.Client.Go.Utilities;
using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Internal
{
    public abstract class ServiceInvoker : IServiceInvoker
    {
        protected readonly IFilterMetadata[] Filters;
        protected FilterCursor Cursor;

        protected RequestExecutingContext RequestExecutingContext;
        protected RequestExecutedContext RequestExecutedContext;
        protected ResultExecutingContext ResultExecutingContext;
        protected ResultExecutedContext ResultExecutedContext;
        protected ExceptionContext ExceptionContext;
        protected object Result;
        private readonly TemplateEngine _templateEngine;

        protected ServiceInvoker(ServiceInvokerContext invokerContext)
        {
            InvokerContext = invokerContext;
            _templateEngine = new TemplateEngine();

            var requestModel = invokerContext.RequestModel;

            Filters = requestModel
                .GetRequestAttributes()
                .OfType<IFilterMetadata>()
                .OrderBy(f =>
                {
                    if (f is IOrderedFilter orderedFilter)
                        return orderedFilter.Order;

                    return 20;
                }).ToArray();
            Cursor = new FilterCursor(Filters);
        }

        protected virtual void BindRabbitContext()
        {
            var requestContext = InvokerContext.RequestContext;
            var rabbitContext = requestContext.RabbitContext;
            var requestModel = InvokerContext.RequestModel;

            ParameterUtil.BuildParameters(requestContext, requestModel);

            var url = _templateEngine.Render(requestContext.RequestUrl, requestContext.PathVariables)?.Result ?? requestContext.RequestUrl;

            var request = rabbitContext.Request;

            var uri = new Uri(url);

            request.Host = uri.Host;
            request.Scheme = uri.Scheme;
            request.Port = uri.Port;
            request.Path = uri.PathAndQuery;

            rabbitContext.Features.Set<IServiceRequestFeature>(new ServiceRequestFeature(rabbitContext.Request)
            {
                RequesType = requestModel.RequesType,
                ResponseType = requestModel.ResponseType
            });
        }

        private Task Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
        {
            switch (next)
            {
                case State.InvokeBegin:
                    {
                        goto case State.ResourceInside;
                    }
                case State.ResourceInside:
                    {
                        goto case State.ExceptionBegin;
                    }
                case State.ExceptionBegin:
                    {
                        Cursor.Reset();
                        goto case State.ExceptionNext;
                    }
                case State.ExceptionNext:
                    {
                        var current = Cursor.GetNextFilter<IExceptionFilter, IAsyncExceptionFilter>();
                        if (current.FilterAsync != null)
                        {
                            state = current.FilterAsync;
                            goto case State.ExceptionAsyncBegin;
                        }
                        else if (current.Filter != null)
                        {
                            state = current.Filter;
                            goto case State.ExceptionSyncBegin;
                        }
                        else if (scope == Scope.Exception)
                        {
                            goto case State.ExceptionInside;
                        }
                        else
                        {
                            goto case State.ActionBegin;
                        }
                    }
                case State.ExceptionAsyncBegin:
                    {
                        var task = InvokeNextExceptionFilterAsync();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ExceptionAsyncResume;
                            return task;
                        }

                        goto case State.ExceptionAsyncResume;
                    }
                case State.ExceptionAsyncResume:
                    {
                        var filter = (IAsyncExceptionFilter)state;
                        var exceptionContext = ExceptionContext;

                        if (exceptionContext?.Exception != null && !exceptionContext.ExceptionHandled)
                        {
                            var task = filter.OnExceptionAsync(exceptionContext);
                            if (task.Status != TaskStatus.RanToCompletion)
                            {
                                next = State.ExceptionAsyncEnd;
                                return task;
                            }

                            goto case State.ExceptionAsyncEnd;
                        }

                        goto case State.ExceptionEnd;
                    }
                case State.ExceptionAsyncEnd:
                    {
                        var exceptionContext = ExceptionContext;

                        if (exceptionContext.Exception == null || exceptionContext.ExceptionHandled)
                        {
                            // We don't need to do anything to trigger a short circuit. If there's another
                            // exception filter on the stack it will check the same set of conditions
                            // and then just skip itself.
                            //todo:拦截器过滤了异常，考虑是否记录日志
                        }

                        goto case State.ExceptionEnd;
                    }
                case State.ExceptionSyncBegin:
                    {
                        var task = InvokeNextExceptionFilterAsync();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ExceptionSyncEnd;
                            return task;
                        }

                        goto case State.ExceptionSyncEnd;
                    }
                case State.ExceptionSyncEnd:
                    {
                        var filter = (IExceptionFilter)state;
                        var exceptionContext = ExceptionContext;

                        // When we get here we're 'unwinding' the stack of exception filters. If we have an unhandled exception,
                        // we'll call the filter. Otherwise there's nothing to do.
                        if (exceptionContext?.Exception != null && !exceptionContext.ExceptionHandled)
                        {
                            filter.OnException(exceptionContext);

                            if (exceptionContext.Exception == null || exceptionContext.ExceptionHandled)
                            {
                                // We don't need to do anything to trigger a short circuit. If there's another
                                // exception filter on the stack it will check the same set of conditions
                                // and then just skip itself.
                                //todo:拦截器过滤了异常，考虑是否记录日志
                            }
                        }

                        goto case State.ExceptionEnd;
                    }
                case State.ExceptionInside:
                    {
                        goto case State.ActionBegin;
                    }
                case State.ExceptionHandled:
                    {
                        // We arrive in this state when an exception happened, but was handled by exception filters
                        // either by setting ExceptionHandled, or nulling out the Exception or setting a result
                        // on the ExceptionContext.
                        //
                        // We need to execute the result (if any) and then exit gracefully which unwinding Resource
                        // filters.

                        if (ExceptionContext.Result == null)
                        {
                            ExceptionContext.Result = null;
                        }

                        Result = ExceptionContext.Result;

                        var task = InvokeAlwaysRunResultFilters();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ResourceInsideEnd;
                            return task;
                        }

                        goto case State.ResourceInsideEnd;
                    }
                case State.ExceptionEnd:
                    {
                        var exceptionContext = ExceptionContext;

                        if (scope == Scope.Exception)
                        {
                            isCompleted = true;
                            return Task.CompletedTask;
                        }

                        if (exceptionContext != null)
                        {
                            if (exceptionContext.Result != null ||
                                exceptionContext.Exception == null ||
                                exceptionContext.ExceptionHandled)
                            {
                                goto case State.ExceptionHandled;
                            }

                            Rethrow(exceptionContext);
                        }

                        var task = InvokeResultFilters();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ResourceInsideEnd;
                            return task;
                        }
                        goto case State.ResourceInsideEnd;
                    }
                case State.ActionBegin:
                    {
                        var task = InvokeInnerFilterAsync();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ActionEnd;
                            return task;
                        }

                        goto case State.ActionEnd;
                    }

                case State.ActionEnd:
                    {
                        if (scope == Scope.Exception)
                        {
                            // If we're inside an exception filter, let's allow those filters to 'unwind' before
                            // the result.
                            isCompleted = true;
                            return Task.CompletedTask;
                        }

                        var task = InvokeResultFilters();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ResourceInsideEnd;
                            return task;
                        }
                        goto case State.ResourceInsideEnd;
                    }
                case State.ResourceInsideEnd:
                    {
                        if (scope == Scope.Result)
                        {
                            goto case State.ActionEnd;
                        }
                        else
                            goto case State.InvokeEnd;
                    }
                case State.InvokeEnd:
                    {
                        isCompleted = true;
                        return Task.CompletedTask;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(next), next, null);
            }
        }

        private async Task InvokeNextExceptionFilterAsync()
        {
            try
            {
                var next = State.ExceptionNext;
                var state = (object)null;
                var scope = Scope.Exception;
                var isCompleted = false;
                while (!isCompleted)
                {
                    await Next(ref next, ref scope, ref state, ref isCompleted);
                }
            }
            catch (Exception exception)
            {
                ExceptionContext = new ExceptionContext(InvokerContext.RequestContext, Filters)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
            }
        }

        private static Task InvokeAlwaysRunResultFilters()
        {
            return Task.CompletedTask;
            /*            var next = State.ResultBegin;
                        var scope = Scope.Invoker;
                        var state = (object)null;
                        var isCompleted = false;
                        while (!isCompleted)
                        {
                            await ResultNext<IAlwaysRunResultFilter, IAsyncAlwaysRunResultFilter>(ref next, ref scope, ref state, ref isCompleted);
                        }
            //            Console.WriteLine("InvokeAlwaysRunResultFilters");
                        return Task.CompletedTask;*/

            /*            while (!isCompleted)
                        {
                            await ResultNext<IAlwaysRunResultFilter, IAsyncAlwaysRunResultFilter>(ref next, ref scope, ref state, ref isCompleted);
                        }*/
        }

        private static void Rethrow(ExceptionContext context)
        {
            if (context == null)
                return;

            if (context.ExceptionHandled)
                return;

            context.ExceptionDispatchInfo?.Throw();

            if (context.Exception != null)
                throw context.Exception;
        }

        private async Task InvokeResultFilters()
        {
            var next = State.ResultBegin;
            var scope = Scope.Invoker;
            var state = (object)null;
            var isCompleted = false;

            while (!isCompleted)
            {
                await ResultNext<IResultFilter, IAsyncResultFilter>(ref next, ref scope, ref state, ref isCompleted);
            }
        }

        private Task ResultNext<TFilter, TFilterAsync>(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
            where TFilter : class, IResultFilter
            where TFilterAsync : class, IAsyncResultFilter
        {
            switch (next)
            {
                case State.ResultBegin:
                    {
                        Cursor.Reset();
                        goto case State.ResultNext;
                    }

                case State.ResultNext:
                    {
                        var current = Cursor.GetNextFilter<TFilter, TFilterAsync>();
                        if (current.FilterAsync != null)
                        {
                            if (ResultExecutingContext == null)
                            {
                                ResultExecutingContext = new ResultExecutingContext(InvokerContext.RequestContext, Filters, Result);
                            }

                            state = current.FilterAsync;
                            goto case State.ResultAsyncBegin;
                        }
                        else if (current.Filter != null)
                        {
                            if (ResultExecutingContext == null)
                            {
                                ResultExecutingContext = new ResultExecutingContext(InvokerContext.RequestContext, Filters, Result);
                            }

                            state = current.Filter;
                            goto case State.ResultSyncBegin;
                        }
                        else
                        {
                            goto case State.ResultInside;
                        }
                    }

                case State.ResultAsyncBegin:
                    {
                        var filter = (TFilterAsync)state;
                        var resultExecutingContext = ResultExecutingContext;

                        var task = filter.OnResultExecutionAsync(resultExecutingContext, InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>);
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ResultAsyncEnd;
                            return task;
                        }

                        goto case State.ResultAsyncEnd;
                    }

                case State.ResultAsyncEnd:
                    {
                        var resultExecutingContext = ResultExecutingContext;
                        var resultExecutedContext = ResultExecutedContext;

                        if (resultExecutedContext == null || resultExecutingContext.Cancel)
                        {
                            ResultExecutedContext = new ResultExecutedContext(
                                InvokerContext.RequestContext,
                                Filters,
                                resultExecutingContext.Result)
                            {
                                Canceled = true,
                            };
                        }

                        goto case State.ResultEnd;
                    }

                case State.ResultSyncBegin:
                    {
                        var filter = (TFilter)state;
                        var resultExecutingContext = ResultExecutingContext;

                        filter.OnResultExecuting(resultExecutingContext);

                        if (ResultExecutingContext.Cancel)
                        {
                            ResultExecutedContext = new ResultExecutedContext(
                                resultExecutingContext,
                                Filters,
                                resultExecutingContext.Result)
                            {
                                Canceled = true,
                            };

                            goto case State.ResultEnd;
                        }

                        var task = InvokeNextResultFilterAsync<TFilter, TFilterAsync>();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ResultSyncEnd;
                            return task;
                        }

                        goto case State.ResultSyncEnd;
                    }

                case State.ResultSyncEnd:
                    {
                        var filter = (TFilter)state;
                        var resultExecutedContext = ResultExecutedContext;

                        filter.OnResultExecuted(resultExecutedContext);

                        goto case State.ResultEnd;
                    }

                case State.ResultInside:
                    {
                        // If we executed result filters then we need to grab the result from there.
                        if (ResultExecutingContext != null)
                        {
                            Result = ResultExecutingContext.Result;
                        }

                        if (Result == null)
                        {
                            // The empty result is always flowed back as the 'executed' result if we don't have one.
                            Result = null;
                        }

                        var task = InvokeResultAsync(Result);
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ResultEnd;
                            return task;
                        }

                        goto case State.ResultEnd;
                    }

                case State.ResultEnd:
                    {
                        var result = Result;
                        isCompleted = true;

                        if (scope == Scope.Result)
                        {
                            if (ResultExecutedContext == null)
                            {
                                ResultExecutedContext = new ResultExecutedContext(InvokerContext.RequestContext, Filters, result);
                            }

                            return Task.CompletedTask;
                        }

                        Rethrow(ResultExecutedContext);
                        return Task.CompletedTask;
                    }

                default:
                    throw new InvalidOperationException(); // Unreachable.
            }
        }

        private async Task InvokeNextResultFilterAsync<TFilter, TFilterAsync>()
            where TFilter : class, IResultFilter
            where TFilterAsync : class, IAsyncResultFilter
        {
            try
            {
                var next = State.ResultNext;
                var state = (object)null;
                var scope = Scope.Result;
                var isCompleted = false;
                while (!isCompleted)
                {
                    await ResultNext<TFilter, TFilterAsync>(ref next, ref scope, ref state, ref isCompleted);
                }
            }
            catch (Exception exception)
            {
                ResultExecutedContext = new ResultExecutedContext(InvokerContext.RequestContext, Filters, Result)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
            }
        }

        private async Task<ResultExecutedContext> InvokeNextResultFilterAwaitedAsync<TFilter, TFilterAsync>()
            where TFilter : class, IResultFilter
            where TFilterAsync : class, IAsyncResultFilter
        {
            if (ResultExecutingContext.Cancel)
            {
                // If we get here, it means that an async filter set cancel == true AND called next().
                // This is forbidden.

                throw new InvalidOperationException("FormatAsyncResultFilter_InvalidShortCircuit");
            }

            await InvokeNextResultFilterAsync<TFilter, TFilterAsync>();

            return ResultExecutedContext;
        }

        protected async Task InvokeResultAsync(object result)
        {
            await InvokerContext.Invoker(InvokerContext.RequestContext.RabbitContext);
        }

        private static void Rethrow(ResultExecutedContext context)
        {
            if (context == null)
                return;

            if (context.ExceptionHandled)
                return;

            context.ExceptionDispatchInfo?.Throw();

            if (context.Exception != null)
                throw context.Exception;
        }

        private enum Scope
        {
            Invoker,
            Resource,
            Exception,
            Result,
        }

        private enum State
        {
            InvokeBegin,
            ResourceInside,
            ResourceInsideEnd,
            ExceptionBegin,
            ExceptionNext,
            ExceptionAsyncBegin,
            ExceptionAsyncResume,
            ExceptionAsyncEnd,
            ExceptionSyncBegin,
            ExceptionSyncEnd,
            ExceptionInside,
            ExceptionHandled,
            ExceptionEnd,
            ActionBegin,
            ActionEnd,
            ResultBegin,
            ResultNext,
            ResultAsyncBegin,
            ResultAsyncEnd,
            ResultSyncBegin,
            ResultSyncEnd,
            ResultInside,
            ResultEnd,
            InvokeEnd,
        }

        protected ServiceInvokerContext InvokerContext { get; }

        protected abstract Task InvokeInnerFilterAsync();

        public virtual async Task InvokeAsync()
        {
            BindRabbitContext();

            try
            {
                var next = State.InvokeBegin;

                // The `scope` tells the `Next` method who the caller is, and what kind of state to initialize to
                // communicate a result. The outermost scope is `Scope.Invoker` and doesn't require any type
                // of context or result other than throwing.
                var scope = Scope.Result;

                // The `state` is used for internal state handling during transitions between states. In practice this
                // means storing a filter instance in `state` and then retrieving it in the next state.
                var state = (object)null;

                // `isCompleted` will be set to true when we've reached a terminal state.
                var isCompleted = false;

                while (!isCompleted)
                {
                    await Next(ref next, ref scope, ref state, ref isCompleted);
                }
            }
            catch (Exception exception)
            {
                ExceptionContext = new ExceptionContext(InvokerContext.RequestContext, Filters)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
        }
    }
}