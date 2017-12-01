using Google.Protobuf;
using Grpc.Core;
using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent.Utilities
{
    public static class MarshallerExtensions
    {
        public static Marshaller<T> CreateGenericMarshaller<T>(this MarshallerModel marshaller)
        {
            return (Marshaller<T>)marshaller.CreateGenericMarshaller();
        }

        public static object CreateGenericMarshaller(this MarshallerModel marshaller)
        {
            //todo: Consider whether you need to cache
            var serializerConstantExpression = Expression.Constant(marshaller.Serializer);
            var parameterExpression = Expression.Parameter(typeof(object));
            var serializerDelegate = Expression.Lambda(Expression.Invoke(serializerConstantExpression, parameterExpression), parameterExpression).Compile();

            var deserializerConstantExpression = Expression.Constant(marshaller.Deserializer);
            var dataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
            var deserializerDelegate = Expression.Lambda(Expression.Convert(Expression.Invoke(deserializerConstantExpression, dataParameterExpression), marshaller.Type), dataParameterExpression).Compile();

            var createMarshallerFactory = GetCreateMarshallerFactory(marshaller.Type);
            return createMarshallerFactory(serializerDelegate, deserializerDelegate);
        }

        #region Private Method

        private static Func<object, object, object> GetCreateMarshallerFactory(Type type)
        {
            return Cache.GetCache(("CreateMarshaller", type), () =>
            {
                var serializerFuncType = Expression.GetFuncType(type, typeof(byte[]));
                var serializerFuncParameterExpression = Expression.Parameter(typeof(object));

                var deserializerFuncType = Expression.GetFuncType(typeof(byte[]), type);
                var deserializerFuncParameterExpression = Expression.Parameter(typeof(object));

                var createCallExpression = Expression.Call(typeof(Marshallers), nameof(Marshallers.Create), new[] { type }, Expression.Convert(serializerFuncParameterExpression, serializerFuncType), Expression.Convert(deserializerFuncParameterExpression, deserializerFuncType));
                return Expression.Lambda<Func<object, object, object>>(createCallExpression, serializerFuncParameterExpression, deserializerFuncParameterExpression).Compile();
            });
        }

        #endregion Private Method

        #region Help Type

        internal class Cache
        {
            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            public static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                    return (T)cache;
                return (T)(Caches[key] = factory());
            }
        }

        #endregion Help Type
    }

    public static class MethodExtensions
    {
        public static IMethod CreateGenericMethod(this MethodModel method)
        {
            var requestType = method.RequestMarshaller.Type;
            var responseType = method.ResponseMarshaller.Type;

            var factory = GetMethodFactory(requestType, responseType);
            return factory(method);
        }

        #region Private Method

        private static Func<MethodModel, IMethod> GetMethodFactory(Type requestType, Type responseType)
        {
            return Cache.GetCache(("MethodFactory", requestType, responseType), () =>
            {
                var methodGenericType = typeof(Method<,>).MakeGenericType(requestType, responseType);

                var methodType = typeof(MethodModel);
                var methodParameterExpression = Expression.Parameter(methodType);

                MemberInfo GetMember(Type type, string name)
                {
                    return type.GetMember(name).First();
                }

                var newExpression = Expression.New(methodGenericType.GetConstructors().Last(),
                    Expression.MakeMemberAccess(methodParameterExpression, GetMember(methodType, nameof(MethodModel.Type))),
                    Expression.MakeMemberAccess(Expression.MakeMemberAccess(methodParameterExpression, GetMember(methodType, nameof(MethodModel.ServiceModel))), GetMember(typeof(ServiceModel), nameof(ServiceModel.ServiceName))),
                    Expression.MakeMemberAccess(methodParameterExpression, GetMember(methodType, nameof(MethodModel.Name))),
                    Expression.Call(typeof(MarshallerExtensions), nameof(MarshallerExtensions.CreateGenericMarshaller), new[] { requestType }, Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(MethodModel.RequestMarshaller)).First())),
                    Expression.Call(typeof(MarshallerExtensions), nameof(MarshallerExtensions.CreateGenericMarshaller), new[] { responseType }, Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(MethodModel.ResponseMarshaller)).First())));

                return Expression.Lambda<Func<MethodModel, IMethod>>(newExpression, methodParameterExpression).Compile();
            });
        }

        #endregion Private Method

        #region Help Type

        internal class Cache
        {
            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            public static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                    return (T)cache;
                return (T)(Caches[key] = factory());
            }
        }

        #endregion Help Type
    }

    public static class FluentUtilities
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

        private static MethodType GetServerMethodType(MethodBase methodInfo)
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

        public static Type GetRequestType(MethodInfo method)
        {
            var provider = method.GetTypeAttribute<IGrpcMethodProvider>();
            var requestType = provider?.RequestType;
            if (requestType != null)
                return requestType;

            var parameters = FilterGrpcParameter(method.GetParameters().Select(i => i.ParameterType)).ToArray();

            if (!parameters.Any())
                return typeof(EmptyRequestModel);

            return parameters.Length == 1 ? parameters[0] : typeof(DynamicRequestModel);
        }

        public static IReadOnlyList<Type> FilterGrpcParameter(IEnumerable<Type> types)
        {
            return types.Where(i => !IsGrpcParameter(i)).ToArray();
        }

        private static bool IsGrpcParameter(Type type)
        {
            return type.Assembly.FullName == typeof(ServerCallContext).Assembly.FullName;
        }

        public static object GetRequestModel(IDictionary<string, object> arguments, IEnumerable<ISerializer> serializers)
        {
            arguments = arguments.Where(i => !IsGrpcParameter(i.Value.GetType())).ToDictionary(i => i.Key, i => i.Value);

            switch (arguments.Count)
            {
                case 0:
                    return new EmptyRequestModel();

                case 1:
                    return arguments.First().Value;

                default:
                    var dictionary = arguments.ToDictionary(i => i.Key, i =>
                    {
                        var value = i.Value;
                        if (value == null)
                            return null;
                        var data = serializers.Serialize(value);
                        if (data == null)
                            throw RpcExceptionUtilities.NotFoundSerializer(value.GetType());
                        return data;
                    }).ToDictionary(i => i.Key, i => i.Value == null || !i.Value.Any() ? ByteString.Empty : ByteString.CopyFrom(i.Value));
                    var request = new DynamicRequestModel();
                    foreach (var item in dictionary)
                        request.Items.Add(item.Key, item.Value);
                    return request;
            }
        }

        public static Type GetResponseType(MethodInfo method)
        {
            var provider = method.GetTypeAttribute<IGrpcMethodProvider>();
            var responseType = provider?.ResponseType;
            return responseType ?? method.GetRealReturnType();
        }

        public static string GetServiceName(Type type)
        {
            var provider = type.GetTypeAttribute<IGrpcServiceNameProvider>();

            var serviceName = provider?.ServiceName;
            if (string.IsNullOrEmpty(serviceName))
                serviceName = $"{type.Namespace.ToLower()}.{type.Name}";

            return serviceName;
        }

        public static string GetMethodName(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var provider = method.GetTypeAttribute<IGrpcMethodProvider>();
            var methodName = provider?.MethodName;
            if (string.IsNullOrEmpty(methodName))
                methodName = method.Name;

            return methodName;
        }

        public static string GetFullServiceName(Type serviceType, MethodInfo method)
        {
            var provider = method.GetTypeAttribute<IGrpcMethodProvider>();
            return provider?.FullName ?? $"/{GetServiceName(serviceType)}/{GetMethodName(method)}";
        }

        public static (string serviceName, string methodName) ResolveServiceNames(string fullName)
        {
            var names = fullName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return (names.ElementAtOrDefault(0), names.ElementAtOrDefault(1));
        }
    }
}