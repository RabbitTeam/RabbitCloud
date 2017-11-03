using Grpc.Core;
using Rabbit.Cloud.Grpc.Server.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Server
{
    public interface IServiceDefinitionCollection : IEnumerable<ServerServiceDefinition>
    {
    }

    internal class ServiceDefinitionCollection : IServiceDefinitionCollection
    {
        private readonly IEnumerable<IServerServiceDefinitionProvider> _providers;
        private IEnumerable<ServerServiceDefinition> _serverServiceDefinitions;

        public ServiceDefinitionCollection(IEnumerable<IServerServiceDefinitionProvider> providers)
        {
            _providers = providers;
        }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ServerServiceDefinition> GetEnumerator()
        {
            if (_serverServiceDefinitions == null)
                _serverServiceDefinitions = GetDefinitions();
            return _serverServiceDefinitions.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Private Method

        private IEnumerable<ServerServiceDefinition> GetDefinitions()
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var serverDelegateCollection = new ServerDelegateCollection();
            foreach (var provider in _providers)
            {
                provider.Collect(serverDelegateCollection);
            }
            foreach (var item in serverDelegateCollection)
            {
                var methodDelegate = item.Delegate;
                var delegateType = methodDelegate.GetType();
                var method = item.Method;
                var requestType = item.RequestType;
                var responseType = item.ResponseType;

                var addMethod = Cache.GetAddMethod(delegateType, method.GetType(), requestType, responseType);
                addMethod.DynamicInvoke(builder, method, methodDelegate);
            }
            return new[] { builder.Build() };
        }

        #endregion Private Method

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

            #endregion Private Method
        }

        #endregion Help Type
    }
}