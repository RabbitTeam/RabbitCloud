using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Filters;
using Rabbit.Go.ApplicationModels;
using Rabbit.Go.Features;
using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Rabbit.Go.Internal
{
    public class RabbitGoInvoker : GoInvoker
    {
        private readonly RabbitRequestDelegate _app;

        public RabbitGoInvoker(RabbitRequestDelegate app, RequestContext requestContext, RequestModel requestModel) : base(requestContext, FilterFactory.GetAllFilters(requestContext, requestModel).ToArray())
        {
            _app = app;
            requestContext.RabbitContext.Features.Set<IGoFeature>(new GoFeature
            {
                RequestContext = requestContext,
                RequestModel = requestModel
            });
        }

        #region Overrides of GoInvoker

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

        #endregion Overrides of GoInvoker

        #region Private Method

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
                            RequestExecutingContext = new RequestExecutingContext(RequestContext, Filters, RequestContext.Arguments);

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
                            RequestExecutedContext = new RequestExecutedContext(RequestContext, Filters, RequestContext.Arguments)
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
                            RequestExecutedContext = new RequestExecutedContext(RequestContext, Filters, RequestContext.Arguments)
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
                                RequestExecutedContext = new RequestExecutedContext(RequestContext, Filters, RequestContext.Arguments)
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
                RequestExecutedContext = new RequestExecutedContext(RequestContext, Filters, RequestContext.Arguments)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception),
                };
            }
        }

        private async Task InvokeActionMethodAsync()
        {
            var rabbitContext = RequestContext.RabbitContext;

            await _app(rabbitContext);

            Result = rabbitContext.Response.Body;
        }

        #endregion Private Method

        #region Help Type

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

        #endregion Help Type
    }
}