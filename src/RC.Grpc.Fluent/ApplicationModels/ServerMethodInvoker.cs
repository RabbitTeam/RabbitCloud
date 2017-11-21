using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public abstract class ServerMethodInvoker : IServerMethodInvoker
    {
        protected virtual ServerMethodModel ServerMethod { get; }
        protected virtual IServiceProvider Services { get; }

        protected ServerMethodInvoker(ServerMethodModel serverMethod, IServiceProvider services)
        {
            ServerMethod = serverMethod;
            Services = services;
        }

        private Func<object[], object> _methodInvoker;

        protected virtual Func<object[], object> MethodInvoker
        {
            get
            {
                if (_methodInvoker != null)
                    return _methodInvoker;

                var parametersParameterExpression = Expression.Parameter(typeof(object[]));

                var methodInfo = ServerMethod.MethodInfo;

                var parameterParameterExpressions = methodInfo.GetParameters().Select(p => Expression.Parameter(p.ParameterType));

                var callExpressions = parameterParameterExpressions.Select((e, index) => Expression.Convert(Expression.ArrayIndex(parametersParameterExpression, Expression.Constant(index)), e.Type));

                var callExpression = Expression.Call(Expression.Constant(GetServiceInstance()), ServerMethod.MethodInfo, callExpressions);

                return _methodInvoker = Expression.Lambda<Func<object[], object>>(callExpression, parametersParameterExpression).Compile();
            }
        }

        #region Implementation of IServerMethodInvoker

        public abstract Task<TResponse> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext callContext);

        public abstract Task<TResponse> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext callContext);

        public abstract Task ServerStreamingServerMethod<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream,
            ServerCallContext callContext);

        public abstract Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream, ServerCallContext callContext);

        #endregion Implementation of IServerMethodInvoker

        protected virtual object GetServiceInstance()
        {
            var serviceInstance = Services.GetRequiredService(ServerMethod.ServerService.Type);
            return serviceInstance;
        }
    }
}