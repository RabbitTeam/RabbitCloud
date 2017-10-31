using Grpc.Core;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities
{
    internal static class ReflectionUtilities
    {
        #region Method Type

        public static MethodType GetMethodType(this MethodInfo methodInfo)
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

        public static Type GetRequestType(this MethodInfo method)
        {
            var parameters = method.GetParameters();

            Type defaultParameterType = null;
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;
                var grpcRequestAttribute = parameter.GetCustomAttribute<GrpcRequestAttribute>();
                if (grpcRequestAttribute != null)
                    return parameterType;

                if (defaultParameterType == null)
                    defaultParameterType = parameterType;
            }

            return defaultParameterType;
        }

        public static string GetServiceName(this Type type)
        {
            var grpcServiceAttribute = type.GetCustomAttribute<GrpcServiceAttribute>();
            var serviceName = grpcServiceAttribute?.ServiceName;
            if (string.IsNullOrEmpty(serviceName))
                serviceName = $"{type.Namespace.ToLower()}.{type.Name}";

            return serviceName;
        }

        public static (string serviceName, string methodName) GetServiceNames(this MethodInfo method)
        {
            var grpcServiceAttribute = method.GetCustomAttribute<GrpcServiceAttribute>();

            if (!string.IsNullOrEmpty(grpcServiceAttribute?.FullName))
                return ResolveServiceNames(grpcServiceAttribute?.FullName);

            var serviceName = grpcServiceAttribute?.ServiceName;
            if (string.IsNullOrEmpty(serviceName))
                serviceName = method.DeclaringType?.GetServiceName();

            var methodName = grpcServiceAttribute?.MethodName;
            if (string.IsNullOrEmpty(methodName))
                methodName = method.Name;

            return (serviceName, methodName);
        }

        public static (string serviceName, string methodName) ResolveServiceNames(string fullName)
        {
            var names = fullName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return (names[0], names[1]);
        }
    }
}