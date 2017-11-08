using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Abstractions.Adapter
{
    public struct CallInvocationDetails
    {
        public CallInvocationDetails(Channel channel, string method, string host, Marshaller requestMarshaller, Marshaller responseMarshaller, CallOptions options)
        {
            Channel = channel;
            Method = method;
            Host = host;
            RequestMarshaller = requestMarshaller;
            ResponseMarshaller = responseMarshaller;
            Options = options;
        }

        /// <summary>
        /// Get channel associated with this call.
        /// </summary>
        public Channel Channel { get; }

        /// <summary>
        /// Gets name of method to be called.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Get name of host.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Gets marshaller used to serialize requests.
        /// </summary>
        public Marshaller RequestMarshaller { get; }

        /// <summary>
        /// Gets marshaller used to deserialized responses.
        /// </summary>
        public Marshaller ResponseMarshaller { get; }

        /// <summary>
        /// Gets the call options.
        /// </summary>
        public CallOptions Options { get; private set; }

        /// <summary>
        /// Returns new instance of <see cref="CallInvocationDetails{TRequest, TResponse}"/> with
        /// <c>Options</c> set to the value provided. Values of all other fields are preserved.
        /// </summary>
        public CallInvocationDetails WithOptions(CallOptions options)
        {
            var newDetails = this;
            newDetails.Options = options;
            return newDetails;
        }
    }

    public class Method : IMethod
    {
        public Method(MethodType type, string fullName, Marshaller requestMarshaller, Marshaller responseMarshaller)
        {
            Type = type;
            FullName = fullName;
            RequestMarshaller = requestMarshaller;
            ResponseMarshaller = responseMarshaller;

            var names = Utilities.ReflectionUtilities.ResolveServiceNames(fullName);

            ServiceName = names.serviceName;
            Name = names.methodName;
        }

        #region Implementation of IMethod

        /// <summary>Gets the type of the method.</summary>
        public MethodType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the name of the service to which this method belongs.
        /// </summary>
        public string ServiceName { get; }

        /// <inheritdoc />
        /// <summary>Gets the unqualified name of the method.</summary>
        public string Name { get; }

        /// <summary>
        /// Gets the fully qualified name of the method. On the server side, methods are dispatched
        /// based on this name.
        /// </summary>
        public string FullName { get; }

        #endregion Implementation of IMethod

        /// <summary>
        /// Gets the marshaller used for request messages.
        /// </summary>
        public Marshaller RequestMarshaller { get; }

        /// <summary>
        /// Gets the marshaller used for response messages.
        /// </summary>
        public Marshaller ResponseMarshaller { get; }
    }

    public class Marshaller
    {
        public Marshaller(Type type, Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            Deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        public Marshaller(Type type, Func<object, byte[]> serializer, Func<byte[], Type, object> deserializer) : this(type, serializer, data => deserializer(data, type))
        {
        }

        /// <summary>
        /// Target Type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the serializer function.
        /// </summary>
        public Func<object, byte[]> Serializer { get; }

        /// <summary>
        /// Gets the deserializer function.
        /// </summary>
        public Func<byte[], object> Deserializer { get; }
    }

    public static class MarshallerExtensions
    {
        public static Marshaller<T> CreateGenericMarshaller<T>(this Marshaller marshaller)
        {
            return (Marshaller<T>)marshaller.CreateGenericMarshaller();
        }

        public static object CreateGenericMarshaller(this Marshaller marshaller)
        {
            //todo: Consider whether you need to cache
            var serializerConstantExpression = Expression.Constant(marshaller.Serializer);
            var parameterExpression = Expression.Parameter(typeof(object));
            var serializerDelegate = Expression.Lambda(Expression.Invoke(serializerConstantExpression, parameterExpression), parameterExpression).Compile();

            var deserializerConstantExpression = Expression.Constant(marshaller.Deserializer);
            var dataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
            var deserializerDelegate = Expression.Lambda(Expression.Convert(Expression.Invoke(deserializerConstantExpression, dataParameterExpression), marshaller.Type), dataParameterExpression).Compile();

            var createMarshallerDelegate = GetCreateMarshallerDelegate(marshaller.Type);
            return createMarshallerDelegate.DynamicInvoke(serializerDelegate, deserializerDelegate);
        }

        #region Private Method

        private static Delegate GetCreateMarshallerDelegate(Type type)
        {
            return Cache.GetCache(("CreateMarshaller", type), () =>
            {
                var serializerFuncType = Expression.GetFuncType(type, typeof(byte[]));
                var serializerFuncParameterExpression = Expression.Parameter(serializerFuncType);

                var deserializerFuncType = Expression.GetFuncType(typeof(byte[]), type);
                var deserializerFuncParameterExpression = Expression.Parameter(deserializerFuncType);

                var createCallExpression = Expression.Call(typeof(Marshallers), nameof(Marshallers.Create),
                    new[] { type }, serializerFuncParameterExpression, deserializerFuncParameterExpression);
                return Expression.Lambda(createCallExpression, serializerFuncParameterExpression,
                    deserializerFuncParameterExpression).Compile();
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
        public static IMethod CreateGenericMethod(this Method method)
        {
            var requestType = method.RequestMarshaller.Type;
            var responseType = method.ResponseMarshaller.Type;

            var factory = GetMethodFactory(requestType, responseType);
            return factory(method);
        }

        #region Private Method

        private static Func<Method, IMethod> GetMethodFactory(Type requestType, Type responseType)
        {
            return Cache.GetCache(("MethodFactory", requestType, responseType), () =>
            {
                var methodGenericType = typeof(Method<,>).MakeGenericType(requestType, responseType);

                var methodType = typeof(Method);
                var methodParameterExpression = Expression.Parameter(methodType);

                var newExpression = Expression.New(methodGenericType.GetConstructors().Last(),
                    Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(Method.Type)).First()),
                    Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(Method.ServiceName)).First()),
                    Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(Method.Name)).First()),
                    Expression.Call(typeof(MarshallerExtensions), nameof(MarshallerExtensions.CreateGenericMarshaller), new[] { requestType }, Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(CallInvocationDetails.RequestMarshaller)).First())),
                    Expression.Call(typeof(MarshallerExtensions), nameof(MarshallerExtensions.CreateGenericMarshaller), new[] { responseType }, Expression.MakeMemberAccess(methodParameterExpression, methodType.GetMember(nameof(CallInvocationDetails.ResponseMarshaller)).First())));

                var factory = Expression.Lambda(newExpression, methodParameterExpression).Compile();

                return new Func<Method, IMethod>(details => (IMethod)factory.DynamicInvoke(details));
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

    public static class CallInvocationDetailsExtensions
    {
        public static object CreateGenericCallInvocationDetails(this CallInvocationDetails details)
        {
            var requestType = details.RequestMarshaller.Type;
            var responseType = details.ResponseMarshaller.Type;

            var factory = GetCallInvocationDetailsFactory(requestType, responseType);
            return factory(details);
        }

        #region Private Method

        private static Func<CallInvocationDetails, object> GetCallInvocationDetailsFactory(Type requestType, Type responseType)
        {
            return Cache.GetCache(("CallInvocationDetailsFactory", requestType, responseType), () =>
            {
                var detailsGenericType = typeof(CallInvocationDetails<,>).MakeGenericType(requestType, responseType);

                var detailsType = typeof(CallInvocationDetails);
                var detailsParameterExpression = Expression.Parameter(detailsType);

                var newExpression = Expression.New(detailsGenericType.GetConstructors().Last(),
                    Expression.MakeMemberAccess(detailsParameterExpression, detailsType.GetMember(nameof(CallInvocationDetails.Channel)).First()),
                    Expression.MakeMemberAccess(detailsParameterExpression, detailsType.GetMember(nameof(CallInvocationDetails.Method)).First()),
                    Expression.MakeMemberAccess(detailsParameterExpression, detailsType.GetMember(nameof(CallInvocationDetails.Host)).First()),
                    Expression.Call(typeof(MarshallerExtensions), nameof(MarshallerExtensions.CreateGenericMarshaller), new[] { requestType }, Expression.MakeMemberAccess(detailsParameterExpression, detailsType.GetMember(nameof(CallInvocationDetails.RequestMarshaller)).First())),
                    Expression.Call(typeof(MarshallerExtensions), nameof(MarshallerExtensions.CreateGenericMarshaller), new[] { responseType }, Expression.MakeMemberAccess(detailsParameterExpression, detailsType.GetMember(nameof(CallInvocationDetails.ResponseMarshaller)).First())),
                    Expression.MakeMemberAccess(detailsParameterExpression, detailsType.GetMember(nameof(CallInvocationDetails.Options)).First()));

                var factory = Expression.Lambda(newExpression, detailsParameterExpression).Compile();

                return new Func<CallInvocationDetails, object>(details => factory.DynamicInvoke(details));
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
}