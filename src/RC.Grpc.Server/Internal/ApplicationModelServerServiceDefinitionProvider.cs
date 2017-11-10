/*using Grpc.Core;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using Rabbit.Cloud.Grpc.Fluent.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Server.Internal
{
    public class ApplicationModelServerServiceDefinitionProviderOptions
    {
        public ICollection<TypeInfo> Types { get; } = new List<TypeInfo>();
    }

    public class ApplicationModelServerServiceDefinitionProvider : IServerServiceDefinitionProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationModelServerServiceDefinitionProviderOptions _options;
        private readonly IReadOnlyCollection<IApplicationModelProvider> _applicationModelProviders;

        public ApplicationModelServerServiceDefinitionProvider(IServiceProvider serviceProvider, IEnumerable<IApplicationModelProvider> applicationModelProviders, IOptions<ApplicationModelServerServiceDefinitionProviderOptions> options)
        {
            if (applicationModelProviders == null)
                throw new ArgumentNullException(nameof(applicationModelProviders));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider;

            _applicationModelProviders = applicationModelProviders.OrderBy(i => i.Order).ToArray();
            _options = options.Value;
        }

        #region Implementation of IServerServiceDefinitionProvider

        public int Order { get; } = 10;

        public void OnProvidersExecuting(ServerServiceDefinitionProviderContext context)
        {
            var applicationModelProviderContext = new ApplicationModelProviderContext(_options.Types);

            foreach (var applicationModelProvider in _applicationModelProviders)
            {
                applicationModelProvider.OnProvidersExecuting(applicationModelProviderContext);
            }

            foreach (var applicationModelProvider in _applicationModelProviders)
            {
                applicationModelProvider.OnProvidersExecuted(applicationModelProviderContext);
            }

            var applicationModel = applicationModelProviderContext.Result;

            var builder = new ServerServiceDefinition.Builder();

            foreach (var serviceModel in applicationModel.ServerServices)
            {
                foreach (var methodModel in serviceModel.Methods)
                {
                    var serverDelegateType = Cache.GetUnaryServerDelegateType(methodModel.RequestMarshaller.Type, methodModel.ResponseMarshaller.Type);

                    var grpcMethod = methodModel.CreateGenericMethod();
                    var methodDelegate = Cache.GetMethodDelegate(serviceModel.ServiceType, methodModel.MethodInfo, serverDelegateType, _serviceProvider, (s, t) => Activator.CreateInstance(t));
                    var addMethod = Cache.GetAddMethod(serverDelegateType, grpcMethod.GetType(), methodModel.RequestMarshaller.Type, methodModel.ResponseMarshaller.Type);
                    addMethod.DynamicInvoke(builder, grpcMethod, methodDelegate);
                }
            }
        }

        public void OnProvidersExecuted(ServerServiceDefinitionProviderContext context)
        {
        }

        #endregion Implementation of IServerServiceDefinitionProvider

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            #endregion Field

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
}*/