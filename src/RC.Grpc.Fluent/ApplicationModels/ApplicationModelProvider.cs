using Grpc.Core;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Fluent.Utilities;
using Rabbit.Cloud.Grpc.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public class ApplicationModelOptions
    {
        public ICollection<TypeInfo> Types { get; } = new List<TypeInfo>();
    }

    public class ApplicationModelHolder
    {
        private ApplicationModel _applicationModel;
        private readonly ApplicationModelOptions _options;
        private readonly IReadOnlyCollection<IApplicationModelProvider> _applicationModelProviders;

        public ApplicationModelHolder(IEnumerable<IApplicationModelProvider> applicationModelProviders, IOptions<ApplicationModelOptions> options, IServiceProvider services)
        {
            if (applicationModelProviders == null)
                throw new ArgumentNullException(nameof(applicationModelProviders));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _applicationModelProviders = applicationModelProviders.OrderBy(i => i.Order).ToArray();
            _options = options.Value;
        }

        public ApplicationModel GetApplicationModel()
        {
            if (_applicationModel != null)
                return _applicationModel;

            return _applicationModel = BuildApplicationModel();
        }

        private ApplicationModel BuildApplicationModel()
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

            return applicationModel;
        }
    }

    public class MethodProvider : IMethodProvider
    {
        private readonly ApplicationModel _applicationModel;

        public MethodProvider(ApplicationModelHolder applicationModelHolder)
        {
            _applicationModel = applicationModelHolder.GetApplicationModel();
        }

        #region Implementation of IMethodProvider

        public int Order { get; } = 10;

        public void OnProvidersExecuting(MethodProviderContext context)
        {
            var applicationModel = _applicationModel;
            foreach (var serviceModel in applicationModel.Services)
            {
                foreach (var methodModel in serviceModel.Methods)
                {
                    var method = methodModel.CreateGenericMethod();
                    context.Results.Add(method);
                }
            }
        }

        public void OnProvidersExecuted(MethodProviderContext context)
        {
        }

        #endregion Implementation of IMethodProvider
    }

    public class ServerServiceDefinitionProvider : IServerServiceDefinitionProvider
    {
        private readonly IServiceProvider _services;
        private readonly ApplicationModel _applicationModel;
        private readonly IMethodTable _methodTable;

        public ServerServiceDefinitionProvider(ApplicationModelHolder applicationModelHolder, IMethodTableProvider methodTableProvider, IServiceProvider services)
        {
            _services = services;
            _applicationModel = applicationModelHolder.GetApplicationModel();
            _methodTable = methodTableProvider.MethodTable;
        }

        #region Implementation of IServerServiceDefinitionProvider

        int IServerServiceDefinitionProvider.Order { get; } = 10;

        //todo: 考虑优化实现
        public void OnProvidersExecuting(ServerServiceDefinitionProviderContext context)
        {
            var applicationModel = _applicationModel;

            var builder = new ServerServiceDefinition.Builder();
            foreach (var serverService in applicationModel.ServerServices)
            {
                foreach (var serverServiceMethod in serverService.ServerMethods)
                {
                    var serviceId = $"/{serverService.ServiceName}/{serverServiceMethod.Method.Name}";

                    var grpcMethod = _methodTable.Get(serviceId);
                    var requestType = serverServiceMethod.Method.RequestMarshaller.Type;
                    var responseType = serverServiceMethod.Method.ResponseMarshaller.Type;

                    var delegateType = Cache.GetUnaryServerDelegateType(requestType, responseType);
                    var methodDelegate = Cache.GetMethodDelegate(serverService.Type, serverServiceMethod.MethodInfo, delegateType, _services,
                        (s, type) =>
                        {
                            var instance = s.GetService(type);
                            return instance ?? Activator.CreateInstance(type);
                        });
                    var addMethodInvoker = Cache.GetAddMethod(delegateType, grpcMethod.GetType(), requestType, responseType);
                    addMethodInvoker(builder, grpcMethod, methodDelegate);
                }
            }
            context.Results.Add(builder.Build());
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

            public static Func<ServerServiceDefinition.Builder, object, object, ServerServiceDefinition.Builder> GetAddMethod(Type delegateType, Type methodType, Type requestType, Type responseType)
            {
                var key = ("AddMethod", delegateType);

                return GetCache(key, () =>
                {
                    var builderParameterExpression = Expression.Parameter(typeof(ServerServiceDefinition.Builder));
                    var methodParameterExpression = Expression.Parameter(typeof(object));
                    var delegateParameterExpression = Expression.Parameter(typeof(object));

                    var callExpression = Expression.Call(builderParameterExpression, "AddMethod",
                        new[] { requestType, responseType },
                        Expression.Convert(methodParameterExpression, methodType), Expression.Convert(delegateParameterExpression, delegateType));

                    return Expression.Lambda<Func<ServerServiceDefinition.Builder, object, object, ServerServiceDefinition.Builder>>(callExpression, builderParameterExpression, methodParameterExpression, delegateParameterExpression).Compile();
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
}