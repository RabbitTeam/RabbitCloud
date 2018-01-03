using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Utilities.Extensions
{
    public static class CallInvokerExtensions
    {
        public static object Call(Type requestType, Type responseType, object request, Channel channel, string method, string host, object requestMarshaller,
            object responseMarshaller, CallOptions callOptions)
        {
            var callInvocationDetailsFactory = Cache.GetCallInvocationDetails(requestType, responseType);

            var callInvocationDetails = callInvocationDetailsFactory(channel, method, host, requestMarshaller, responseMarshaller, callOptions);

            var invoker = Cache.GetUnaryCallInvoker(requestType, responseType);
            return invoker(callInvocationDetails, request);
        }

        #region Help Type

        private static class Cache
        {
            private static readonly ConcurrentDictionary<(Type requestType, Type responseType), Func<Channel, string, string, object, object, CallOptions, object>> CallInvocationDetailsFactorys = new ConcurrentDictionary<(Type requestType, Type responseType), Func<Channel, string, string, object, object, CallOptions, object>>();

            public static Func<Channel, string, string, object, object, CallOptions, object> GetCallInvocationDetails(Type requestType, Type responseType)
            {
                return CallInvocationDetailsFactorys.GetOrAdd((requestType, responseType), key =>
                {
                    var channelParameterExpression = Expression.Parameter(typeof(Channel), "channel");
                    var methodParameterExpression = Expression.Parameter(typeof(string), "method");
                    var hostParameterExpression = Expression.Parameter(typeof(string), "host");
                    var requestMarshallerParameterExpression = Expression.Parameter(typeof(object), "requestMarshaller");
                    var responseMarshallerParameterExpression = Expression.Parameter(typeof(object), "responseMarshaller");
                    var callOptionsParameterExpression = Expression.Parameter(typeof(CallOptions), "callOptions");

                    var type = typeof(CallInvocationDetails<,>).MakeGenericType(requestType, responseType);
                    var newExpression = Expression.New(type.GetConstructors().Last(),
                        channelParameterExpression,
                        methodParameterExpression,
                        hostParameterExpression,
                        Expression.Convert(requestMarshallerParameterExpression, typeof(Marshaller<>).MakeGenericType(requestType)),
                        Expression.Convert(responseMarshallerParameterExpression, typeof(Marshaller<>).MakeGenericType(responseType)),
                        callOptionsParameterExpression);

                    return Expression.Lambda<Func<Channel, string, string, object, object, CallOptions, object>>(Expression.Convert(newExpression, typeof(object)), channelParameterExpression,
                        methodParameterExpression, hostParameterExpression,
                        requestMarshallerParameterExpression,
                        responseMarshallerParameterExpression,
                        callOptionsParameterExpression).Compile();
                });
            }

            private static readonly ConcurrentDictionary<(Type requestType, Type responseType), Func<object, object, object>> UnaryCallInvokers = new ConcurrentDictionary<(Type requestType, Type responseType), Func<object, object, object>>();

            public static Func<object, object, object> GetUnaryCallInvoker(Type requestType, Type responseType)
            {
                return UnaryCallInvokers.GetOrAdd((requestType, responseType), key =>
                {
                    var callInvocationDetailsParameterExpression = Expression.Parameter(typeof(object), "callInvocationDetails");
                    var requestParameterExpression = Expression.Parameter(typeof(object), "request");

                    var callExpression = Expression.Call(typeof(Calls), nameof(Calls.AsyncUnaryCall), new[] { requestType, responseType }, Expression.Convert(callInvocationDetailsParameterExpression, typeof(CallInvocationDetails<,>).MakeGenericType(requestType, responseType)), Expression.Convert(requestParameterExpression, requestType));

                    return Expression.Lambda<Func<object, object, object>>(callExpression, callInvocationDetailsParameterExpression, requestParameterExpression).Compile();
                });
            }
        }

        #endregion Help Type
    }
}