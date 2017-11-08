using Grpc.Core;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions.Adapter;
using Rabbit.Cloud.Grpc.Abstractions.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Server.Internal
{
    public class ServerServiceDefinitionProviderOptions
    {
        public IDictionary<Type, MethodInfo[]> Entries { get; set; }

        public Func<IServiceProvider, Type, object> Factory { get; set; }
    }

    public class DefaultServerServiceDefinitionProvider : IServerServiceDefinitionProvider
    {
        #region Field

        private readonly ServerServiceDefinitionProviderOptions _options;
        private readonly IGrpcServiceDescriptorCollection _methodCollection;
        private readonly IServiceProvider _services;

        #endregion Field

        #region Constructor

        public DefaultServerServiceDefinitionProvider(IOptions<ServerServiceDefinitionProviderOptions> options, IGrpcServiceDescriptorCollection methodCollection, IServiceProvider services)
        {
            _options = options.Value;
            _methodCollection = methodCollection;
            _services = services;
        }

        #endregion Constructor

        #region Implementation of IServerServiceDefinitionProvider

        public void Collect(IServerMethodCollection serverMethods)
        {
            if (_options.Entries == null || !_options.Entries.Any())
                return;

            foreach (var entry in _options.Entries)
            {
                var type = entry.Key;
                foreach (var methodInfo in entry.Value)
                {
                    var descriptor = GrpcServiceDescriptor.Create(type, methodInfo);

                    var serviceMethod = serverMethods.Get(descriptor.ServiceId);
                    // ignore repeated additions
                    if (serviceMethod != null)
                        continue;

                    var method = _methodCollection.Get(descriptor.ServiceId);
                    var requestType = method.RequestMarshaller.Type;
                    var responseType = method.ResponseMarshaller.Type;

                    var delegateType = Cache.GetUnaryServerDelegateType(requestType, responseType);

                    var methodDelegate = Cache.GetMethodDelegate(type, methodInfo, delegateType, _services, _options.Factory);

                    serverMethods.Set(new ServiceMethod
                    {
                        Delegate = methodDelegate,
                        Method = new Method(MethodType.Unary, method.ServiceId, method.RequestMarshaller, method.ResponseMarshaller).CreateGenericMethod(),
                        RequestType = requestType,
                        ResponseType = responseType
                    });
                }
            }
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

            public static Delegate GetMethodDelegate(Type serviceType, MethodInfo methodInfo, Type delegateType, IServiceProvider services, Func<IServiceProvider, Type, object> instanceFactory)
            {
                //todo: 考虑优化，MethodDelegate是固定的，服务实例不是固定的
                var key = ("MethodDelegate", serviceType, delegateType);
                return GetCache(key, () =>
                {
                    var parameters = methodInfo.GetParameters();

                    IList<ParameterExpression> parameterExpressions = parameters.Select(i => GetParameterExpression(i.ParameterType)).ToList();

                    var missServerCallContext = parameters.All(i => i.ParameterType != typeof(ServerCallContext));

                    var instanceExpression = GetInstanceExpression(serviceType, services, instanceFactory);
                    var callExpression = Expression.Call(instanceExpression, methodInfo, parameterExpressions);

                    //todo:需要考虑非 UnaryServerMethod<> 委托类型的参数选择
                    if (missServerCallContext)
                        parameterExpressions.Add(GetParameterExpression(typeof(ServerCallContext)));

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

            private static Expression GetInstanceExpression(Type type, IServiceProvider serviceProvider, Func<IServiceProvider, Type, object> factory)
            {
                var key = ("instanceFactory", type);
                return GetCache(key, () =>
                {
                    var instancExpression = Expression.Invoke(Expression.Constant(factory), Expression.Constant(serviceProvider), Expression.Constant(type));
                    var serviceInstanceExpression = Expression.Convert(instancExpression, type);

                    return Expression.Invoke(Expression.Lambda(serviceInstanceExpression));
                });
            }

            #endregion Private Method
        }

        #endregion Help Type
    }
}