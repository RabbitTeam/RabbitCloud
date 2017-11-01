using Grpc.Core;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Server
{
    public class DefaultServerServiceDefinitionProviderOptions
    {
        public IEnumerable<Type> Types { get; set; }

        public Func<Type, object> Factory { get; set; }
    }

    public class DefaultServerServiceDefinitionProvider : IServerServiceDefinitionProvider
    {
        #region Field

        private readonly DefaultServerServiceDefinitionProviderOptions _options;
        private readonly IMethodCollection _methodCollection;

        #endregion Field

        #region Constructor

        public DefaultServerServiceDefinitionProvider(DefaultServerServiceDefinitionProviderOptions options, IMethodCollection methodCollection)
        {
            _options = options;
            _methodCollection = methodCollection;
        }

        #endregion Constructor

        #region Implementation of IServerServiceDefinitionProvider

        public IEnumerable<ServerServiceDefinition> GetDefinitions()
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            foreach (var type in _options.Types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var methodInfo in methods)
                {
                    var descriptor = GrpcServiceDescriptor.Create(methodInfo);

                    var method = _methodCollection.Get(descriptor.ServiceId);
                    var requestType = descriptor.RequesType;
                    var responseType = descriptor.ResponseType;

                    var delegateType = Cache.GetUnaryServerDelegateType(requestType, responseType);

                    var addMethod = Cache.GetAddMethod(delegateType, method.GetType(), requestType, responseType);
                    var methodDelegate = Cache.GetMethodDelegate(methodInfo, delegateType, _options.Factory);
                    addMethod.DynamicInvoke(builder, method, methodDelegate);
                }
            }

            yield return builder.Build();
        }

        #endregion Implementation of IServerServiceDefinitionProvider

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            #endregion Field

            public static Type GetUnaryServerDelegateType(Type requestType, Type responseType)
            {
                var key = ("UnaryServerDelegateType", requestType, responseType);
                return GetCache(key, () => typeof(UnaryServerMethod<,>).MakeGenericType(requestType, responseType));
            }

            public static Delegate GetAddMethod(Type delegateType, Type methodType, Type requestType, Type responseType)
            {
                var key = ("AddMethod", delegateType);

                return GetCache(key, () =>
                {
                    var builderParameterExpression = GetParameterExpression(typeof(ServerServiceDefinition.Builder));
                    var methodParameterExpression = GetParameterExpression(methodType);
                    var delegateParameterExpression = GetParameterExpression(delegateType);

                    var callExpression = Expression.Call(builderParameterExpression, "AddMethod",
                        new[] { requestType, responseType },
                        methodParameterExpression, delegateParameterExpression);

                    return Expression.Lambda(callExpression, builderParameterExpression, methodParameterExpression, delegateParameterExpression).Compile();
                });
            }

            public static Delegate GetMethodDelegate(MethodInfo methodInfo, Type delegateType, Func<Type, object> instanceFactory)
            {
                var key = ("MethodDelegate", delegateType);
                return GetCache(key, () =>
                {
                    var type = methodInfo.DeclaringType;
                    var parameterExpressions = methodInfo.GetParameters().Select(i => GetParameterExpression(i.ParameterType)).ToArray();

                    var instanceExpression = GetInstanceExpression(type, instanceFactory);
                    var callExpression = Expression.Call(instanceExpression, methodInfo, parameterExpressions.Cast<Expression>());
                    var methodDelegate = Expression.Lambda(delegateType, callExpression, parameterExpressions).Compile();
                    return methodDelegate;
                });
            }

            #region Private Method

            private static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                {
                    return (T)cache;
                }
                return (T)(Caches[key] = factory());
            }

            private static ParameterExpression GetParameterExpression(Type type)
            {
                var key = ("Parameter", type);

                return GetCache(key, () => Expression.Parameter(type));
            }

            private static Expression GetInstanceExpression(Type type, Func<Type, object> factory)
            {
                var key = ("instanceFactory", type);
                return GetCache(key, () =>
                {
                    var instancExpression = Expression.Invoke(Expression.Constant(factory), Expression.Constant(type));
                    var serviceInstanceExpression = Expression.Convert(instancExpression, type);

                    return Expression.Invoke(Expression.Lambda(serviceInstanceExpression));
                });
            }

            #endregion Private Method
        }

        #endregion Help Type
    }
}