using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Utilities.Extensions
{
    public static class CallInvokerExtensions
    {
        #region Field

        private static readonly ParameterExpression CallInvokerParameterExpression;
        private static readonly ParameterExpression HostParameterExpression;
        private static readonly ParameterExpression CallOptionsParameterExpression;

        #endregion Field

        #region Constructor

        static CallInvokerExtensions()
        {
            CallInvokerParameterExpression = Expression.Parameter(typeof(CallInvoker), "invoker");
            HostParameterExpression = Expression.Parameter(typeof(string), "host");
            CallOptionsParameterExpression = Expression.Parameter(typeof(CallOptions), "callOptions");
        }

        #endregion Constructor

        public static object BlockingUnaryCall(this CallInvoker callInvoker, IMethod method, string host, CallOptions callOptions, object request)
        {
            return callInvoker.Call(nameof(CallInvoker.BlockingUnaryCall), method, host, callOptions, request);
        }

        public static object Call(this CallInvoker callInvoker, IMethod method, string host, CallOptions callOptions, object request)
        {
            return callInvoker.Call(GetCallMethodName(method), method, host, callOptions, request);
        }

        public static object Call(this CallInvoker callInvoker, string callMethodName, IMethod method, string host, CallOptions callOptions, object request)
        {
            var invoker = Cache.GetInvoker(method, callMethodName);
            return invoker(callInvoker, method, host, callOptions, request);
        }

        #region Private Method

        private static string GetCallMethodName(IMethod method)
        {
            switch (method.Type)
            {
                case MethodType.Unary:
                    return nameof(CallInvoker.AsyncUnaryCall);

                case MethodType.ClientStreaming:
                    return nameof(CallInvoker.AsyncClientStreamingCall);

                case MethodType.ServerStreaming:
                    return nameof(CallInvoker.AsyncServerStreamingCall);

                case MethodType.DuplexStreaming:
                    return nameof(CallInvoker.AsyncDuplexStreamingCall);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion Private Method

        #region Help Type

        private static class Cache
        {
            private static readonly ConcurrentDictionary<string, Func<CallInvoker, IMethod, string, CallOptions, object, object>> Caches = new ConcurrentDictionary<string, Func<CallInvoker, IMethod, string, CallOptions, object, object>>();

            public static Func<CallInvoker, IMethod, string, CallOptions, object, object> GetInvoker(IMethod method, string callMethodName)
            {
                var key = method.FullName;

                if (Caches.TryGetValue(key, out var invoker))
                    return invoker;

                var typeArguments = method.GetType().GenericTypeArguments;
                var requestType = typeArguments[0];
                var requestParameterExpression = Expression.Parameter(typeof(object));
                var methodParameterExpression = Expression.Parameter(typeof(IMethod));

                Expression[] parameterExpressions;
                IEnumerable<ParameterExpression> lambdaParameterExpressions;

                switch (callMethodName)
                {
                    case nameof(CallInvoker.AsyncClientStreamingCall):
                    case nameof(CallInvoker.AsyncDuplexStreamingCall):
                        parameterExpressions = new Expression[]
                        {
                                methodParameterExpression,
                                HostParameterExpression,
                                CallOptionsParameterExpression
                        };
                        lambdaParameterExpressions = new[]
                        {
                                CallInvokerParameterExpression,
                                methodParameterExpression,
                                HostParameterExpression,
                                CallOptionsParameterExpression
                            };
                        break;

                    default:
                        parameterExpressions = new Expression[]
                        {
                                Expression.Convert(methodParameterExpression,method.GetType()),
                                HostParameterExpression,
                                CallOptionsParameterExpression,
                                Expression.Convert(requestParameterExpression,requestType)
                        };
                        lambdaParameterExpressions = new[]
                        {
                                CallInvokerParameterExpression,
                                methodParameterExpression,
                                HostParameterExpression,
                                CallOptionsParameterExpression,
                                requestParameterExpression
                            };
                        break;
                }

                var callExpression = Expression.Call(CallInvokerParameterExpression, callMethodName, typeArguments, parameterExpressions);

                var lambda = Expression.Lambda<Func<CallInvoker, IMethod, string, CallOptions, object, object>>(callExpression, lambdaParameterExpressions);
                invoker = lambda.Compile();

                Caches.TryAdd(key, invoker);
                return invoker;
            }
        }

        #endregion Help Type
    }
}