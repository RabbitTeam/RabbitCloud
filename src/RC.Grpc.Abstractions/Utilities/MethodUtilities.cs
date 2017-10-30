using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities
{
    public static class MarshallerUtilities
    {
        private static readonly ParameterExpression DataParameterExpression;

        static MarshallerUtilities()
        {
            DataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
        }

        public static object CreateMarshaller(Type type, Expression serializerDelegateExpression, Expression deserializerDelegateExpression)
        {
            var createMethodExpression = Expression.Call(typeof(Marshallers), nameof(Marshallers.Create), new[] { type }, serializerDelegateExpression, deserializerDelegateExpression);

            var factory = Expression.Lambda(createMethodExpression).Compile();
            return factory.DynamicInvoke();
        }

        public static object CreateMarshaller(Type type, Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            var requestParameterExpression = Expression.Parameter(type);
            var objectRequestParameterExpression = Expression.Convert(requestParameterExpression, typeof(object));
            var serializerCallExpression = Expression.Call(Expression.Constant(serializer.Target), serializer.Method, objectRequestParameterExpression);

            var serializerDelegate = Expression.Lambda(serializerCallExpression, requestParameterExpression).Compile();
            var serializerDelegateExpression = Expression.Constant(serializerDelegate);

            var dataParameterExpression = DataParameterExpression;
            var deserializerCallExpression = Expression.Convert(Expression.Call(Expression.Constant(deserializer.Target), deserializer.Method, dataParameterExpression), type);

            var deserializerDelegate = Expression.Lambda(deserializerCallExpression, dataParameterExpression).Compile();
            var deserializerDelegateExpression = Expression.Constant(deserializerDelegate);

            return CreateMarshaller(type, serializerDelegateExpression, deserializerDelegateExpression);
        }
    }

    public static class MethodUtilities
    {
        #region Method Type

        public static MethodType GetMethodType(MethodInfo methodInfo)
        {
            var serverCallContextType = typeof(ServerCallContext);
            return methodInfo.GetParameters().Any(i => i.ParameterType == serverCallContextType)
                ? GetClientMethodType(methodInfo)
                : GetServerMethodType(methodInfo);
        }

        private static MethodType GetClientMethodType(MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;

            var genericTypeDefinition = returnType.IsGenericType ? returnType.GetGenericTypeDefinition() : null;

            if (genericTypeDefinition == typeof(AsyncUnaryCall<>))
            {
                return MethodType.Unary;
            }
            if (genericTypeDefinition == typeof(AsyncClientStreamingCall<,>))
            {
                return MethodType.ClientStreaming;
            }
            if (genericTypeDefinition == typeof(AsyncServerStreamingCall<>))
            {
                return MethodType.ServerStreaming;
            }
            if (genericTypeDefinition == typeof(AsyncDuplexStreamingCall<,>))
            {
                return MethodType.DuplexStreaming;
            }
            return MethodType.Unary;
        }

        private static MethodType GetServerMethodType(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(i => i.ParameterType);

            var containsAsyncStreamReader = false;
            var containsServerStreamWriter = false;

            foreach (var parameterType in parameterTypes)
            {
                if (!parameterType.IsGenericType)
                    continue;
                var genericTypeDefinition = parameterType.GetGenericTypeDefinition();

                if (!containsAsyncStreamReader && genericTypeDefinition == typeof(IAsyncStreamReader<>))
                {
                    containsAsyncStreamReader = true;
                }
                if (!containsServerStreamWriter && genericTypeDefinition == typeof(IAsyncStreamWriter<>))
                {
                    containsServerStreamWriter = true;
                }
                if (containsAsyncStreamReader && containsServerStreamWriter)
                    break;
            }

            if (containsAsyncStreamReader && containsServerStreamWriter)
                return MethodType.DuplexStreaming;
            if (containsServerStreamWriter)
                return MethodType.ServerStreaming;
            if (containsAsyncStreamReader)
                return MethodType.ClientStreaming;
            return MethodType.Unary;
        }

        #endregion Method Type

        #region Create Method

        public static IMethod CreateMethod(MethodInfo method, object requestMarshaller, object responseMarshaller)
        {
            var type = method.DeclaringType;
            var serviceName = $"{type.Namespace.ToLower()}.{type.Name}";

            return CreateMethod(serviceName, method, requestMarshaller, responseMarshaller);
        }

        public static IMethod CreateMethod(string serviceName, MethodInfo method, object requestMarshaller, object responseMarshaller)
        {
            var requesType = method.GetParameters().FirstOrDefault()?.ParameterType;
            var responseType = method.ReturnType;

            if (typeof(Task).IsAssignableFrom(responseType) && responseType.IsGenericType)
            {
                responseType = responseType.GetGenericArguments().First();
            }

            return CreateMethod(serviceName, method, requesType, responseType, requestMarshaller, responseMarshaller);
        }

        private static IMethod CreateMethod(string serviceName, MethodInfo method, Type requesType, Type responseType, object requestMarshaller, object responseMarshaller)
        {
            var methodName = method.Name;
            var methodType = GetMethodType(method);

            return CreateMethod(serviceName, methodName, methodType, requesType, responseType, requestMarshaller, responseMarshaller);
        }

        public static IMethod CreateMethod(string serviceName, string methodName, MethodType methodType, Type requesType, Type responseType, object requestMarshaller, object responseMarshaller)
        {
            var constructor = Cache.GetConstructor(requesType, responseType);

            var methodTypeExpression = Cache.GetConstant(methodType);
            var serviceNameExpression = Expression.Constant(serviceName);
            var methodNameExpression = Expression.Constant(methodName);
            var requestMarshallerExpression = Expression.Constant(requestMarshaller);
            var responseMarshallerExpression = Expression.Constant(responseMarshaller);

            var newExpression = Expression.New(constructor, methodTypeExpression, serviceNameExpression, methodNameExpression, requestMarshallerExpression, responseMarshallerExpression);

            var factory = Expression.Lambda(newExpression).Compile();

            return (IMethod)factory.DynamicInvoke();
        }

        #endregion Create Method

        #region Help Type

        private static class Cache
        {
            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            private static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                {
                    return (T)cache;
                }
                return (T)(Caches[key] = factory());
            }

            public static ConstructorInfo GetConstructor(Type requesType, Type responseType)
            {
                var key = ("constructor", requesType, responseType);

                return GetCache(key, () => typeof(Method<,>).MakeGenericType(requesType, responseType).GetConstructors().First());
            }

            public static ConstantExpression GetConstant(object value)
            {
                var key = ("constant", value);
                return GetCache(key, () => Expression.Constant(value));
            }
        }

        #endregion Help Type
    }
}