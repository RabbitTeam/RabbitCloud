using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Internal
{
    public class DefaultServiceInvoker : ServiceInvoker
    {
        public DefaultServiceInvoker(ServiceInvokerContext invokerContext) : base(invokerContext)
        {
        }

        #region Implementation of IServiceInvoker

        private Task Next(ref State next, ref Scope scope, ref object state, ref bool isCompleted)
        {
            switch (next)
            {
                case State.ActionBegin:
                    {
                        Cursor.Reset();
                        goto case State.ActionNext;
                    }

                case State.ActionNext:
                    {
                        var current = Cursor.GetNextFilter<IRequestFilter, IAsyncRequestFilter>();

                        if (RequestExecutingContext == null && (current.Filter != null || current.FilterAsync != null))
                            RequestExecutingContext = new RequestExecutingContext(InvokerContext.RequestContext, Filters, InvokerContext.RequestContext.Arguments);

                        if (current.FilterAsync != null)
                        {
                            state = current.FilterAsync;

                            goto case State.ActionAsyncBegin;
                        }

                        if (current.Filter != null)
                        {
                            state = current.Filter;

                            goto case State.ActionSyncBegin;
                        }
                        goto case State.ActionInside;
                    }

                case State.ActionAsyncBegin:
                    {
                        var filter = (IAsyncRequestFilter)state;
                        var requestExecutingContext = RequestExecutingContext;
                        var task = filter.OnRequestExecutionAsync(requestExecutingContext, InvokeNextActionFilterAwaitedAsync);
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ActionAsyncEnd;
                            return task;
                        }

                        goto case State.ActionAsyncEnd;
                    }

                case State.ActionAsyncEnd:
                    {
                        if (RequestExecutedContext == null)
                        {
                            RequestExecutedContext = new RequestExecutedContext(InvokerContext.RequestContext, Filters, InvokerContext.RequestContext.Arguments)
                            {
                                Canceled = true,
                                Result = RequestExecutingContext.Result
                            };
                        }
                        goto case State.ActionEnd;
                    }

                case State.ActionSyncBegin:
                    {
                        var filter = (IRequestFilter)state;
                        var actionExecutingContext = RequestExecutingContext;

                        filter.OnRequestExecuting(actionExecutingContext);

                        if (actionExecutingContext?.Result != null)
                        {
                            RequestExecutedContext = new RequestExecutedContext(InvokerContext.RequestContext, Filters, InvokerContext.RequestContext.Arguments)
                            {
                                Canceled = true,
                                Result = actionExecutingContext.Result
                            };

                            goto case State.ActionEnd;
                        }

                        var task = InvokeNextActionFilterAsync();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ActionSyncEnd;
                            return task;
                        }

                        goto case State.ActionSyncEnd;
                    }

                case State.ActionSyncEnd:
                    {
                        var filter = (IRequestFilter)state;
                        var actionExecutedContext = RequestExecutedContext;

                        filter.OnRequestExecuted(actionExecutedContext);

                        goto case State.ActionEnd;
                    }

                case State.ActionInside:
                    {
                        var task = InvokeActionMethodAsync();
                        if (task.Status != TaskStatus.RanToCompletion)
                        {
                            next = State.ActionEnd;
                            return task;
                        }

                        goto case State.ActionEnd;
                    }

                case State.ActionEnd:
                    {
                        if (scope == Scope.Action)
                        {
                            if (RequestExecutedContext == null)
                            {
                                RequestExecutedContext = new RequestExecutedContext(InvokerContext.RequestContext, Filters, InvokerContext.RequestContext.Arguments)
                                {
                                    Result = Result
                                };
                            }

                            isCompleted = true;
                            return Task.CompletedTask;
                        }

                        var actionExecutedContext = RequestExecutedContext;
                        Rethrow(actionExecutedContext);

                        if (actionExecutedContext != null)
                        {
                            Result = actionExecutedContext.Result;
                        }

                        isCompleted = true;
                        return Task.CompletedTask;
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private static void Rethrow(RequestExecutedContext context)
        {
            if (context == null)
                return;

            if (context.ExceptionHandled)
                return;

            context.ExceptionDispatchInfo?.Throw();

            if (context.Exception != null)
                throw context.Exception;
        }

        private async Task<RequestExecutedContext> InvokeNextActionFilterAwaitedAsync()
        {
            if (RequestExecutingContext.Result != null)
            {
                throw new InvalidOperationException("FormatAsyncActionFilter_InvalidShortCircuit");
            }

            await InvokeNextActionFilterAsync();

            return RequestExecutedContext;
        }

        private async Task InvokeNextActionFilterAsync()
        {
            try
            {
                var next = State.ActionNext;
                var state = (object)null;
                var scope = Scope.Action;
                var isCompleted = false;
                while (!isCompleted)
                {
                    await Next(ref next, ref scope, ref state, ref isCompleted);
                }
            }
            catch (Exception exception)
            {
                RequestExecutedContext = new RequestExecutedContext(this.InvokerContext.RequestContext, Filters, InvokerContext.RequestContext.Arguments)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
            }
        }

        private async Task InvokeActionMethodAsync()
        {
            var rabbitContext = InvokerContext.RequestContext.RabbitContext;
            await InvokerContext.Invoker(rabbitContext);

            Result = rabbitContext.Response.Body;
        }

        #endregion Implementation of IServiceInvoker

        private enum Scope
        {
            Invoker,
            Action,
        }

        private enum State
        {
            ActionBegin,
            ActionNext,
            ActionAsyncBegin,
            ActionAsyncEnd,
            ActionSyncBegin,
            ActionSyncEnd,
            ActionInside,
            ActionEnd,
        }

        #region Overrides of ServiceInvoker

        protected override async Task InvokeInnerFilterAsync()
        {
            var next = State.ActionBegin;
            var scope = Scope.Invoker;
            var state = (object)null;
            var isCompleted = false;

            while (!isCompleted)
            {
                await Next(ref next, ref scope, ref state, ref isCompleted);
            }
        }

        #endregion Overrides of ServiceInvoker
    }
}