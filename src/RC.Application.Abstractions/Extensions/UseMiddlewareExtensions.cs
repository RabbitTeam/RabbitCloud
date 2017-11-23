using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Application.Abstractions.Extensions
{
    public static class UseMiddlewareExtensions
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";

        private static readonly MethodInfo GetServiceInfo = typeof(UseMiddlewareExtensions).GetMethod(nameof(GetService), BindingFlags.NonPublic | BindingFlags.Static);

        public static IRabbitApplicationBuilder UseMiddleware<TMiddleware>(this IRabbitApplicationBuilder app, params object[] args)
        {
            return app.UseMiddleware(typeof(TMiddleware), args);
        }

        public static IRabbitApplicationBuilder UseMiddleware(this IRabbitApplicationBuilder app, Type middleware, params object[] args)
        {
            if (typeof(IRabbitMiddleware).GetTypeInfo().IsAssignableFrom(middleware.GetTypeInfo()))
            {
                if (args.Length > 0)
                    throw new NotSupportedException($"{typeof(IRabbitMiddleware)} not supported args.");

                return UseMiddlewareInterface(app, middleware);
            }

            var applicationServices = app.ApplicationServices;

            return app.Use(next =>
            {
                var methods = middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                var invokeMethods = methods.Where(m => string.Equals(m.Name, InvokeMethodName, StringComparison.Ordinal) || string.Equals(m.Name, InvokeAsyncMethodName, StringComparison.Ordinal)).ToArray();

                if (invokeMethods.Length > 1)
                {
                    throw new InvalidOperationException($"UseMiddleMutlipleInvokes {InvokeMethodName} {InvokeAsyncMethodName}");
                }

                if (invokeMethods.Length == 0)
                {
                    throw new InvalidOperationException($"UseMiddlewareNoInvokeMethod({InvokeMethodName}, {InvokeAsyncMethodName})");
                }

                var methodinfo = invokeMethods[0];
                if (!typeof(Task).IsAssignableFrom(methodinfo.ReturnType))
                {
                    throw new InvalidOperationException($"UseMiddlewareNonTaskReturnType({InvokeMethodName}, {InvokeAsyncMethodName}, {nameof(Task)})");
                }

                var parameters = methodinfo.GetParameters();
                if (parameters.Length == 0 || !typeof(IRabbitContext).IsAssignableFrom(parameters[0].ParameterType))
                {
                    throw new InvalidOperationException($"UseMiddlewareNoParameters({InvokeMethodName}, {InvokeAsyncMethodName}, {typeof(IRabbitContext).Name})");
                }

                var ctorArgs = new object[args.Length + 1];
                ctorArgs[0] = next;
                Array.Copy(args, 0, ctorArgs, 1, args.Length);
                var instance = ActivatorUtilities.CreateInstance(app.ApplicationServices, middleware, ctorArgs);
                if (parameters.Length == 1)
                {
                    return (RabbitRequestDelegate)methodinfo.CreateDelegate(typeof(RabbitRequestDelegate), instance);
                }
                var factory = Compile<object>(methodinfo, parameters);
                return context =>
                {
                    var serviceProvider = GetServices(applicationServices, context);

                    if (serviceProvider == null)
                    {
                        throw new InvalidOperationException($"UseMiddlewareIServiceProviderNotAvailable {nameof(IServiceProvider)}");
                    }

                    return factory(instance, context, serviceProvider);
                };
            });
        }

        #region Private Method

        private static IRabbitApplicationBuilder UseMiddlewareInterface(IRabbitApplicationBuilder app, Type middlewareType)
        {
            return app.Use(next =>
            {
                return async context =>
                {
                    var middleware = (IRabbitMiddleware)GetServices(app.ApplicationServices, context).GetService(middlewareType);
                    if (middleware == null)
                    {
                        throw new InvalidOperationException($"UseMiddlewareUnableToCreateMiddleware {middlewareType}");
                    }

                    await middleware.InvokeAsync(context, next);
                };
            });
        }

        private static IServiceProvider GetServices(IServiceProvider applicationServices, IRabbitContext context)
        {
            var serviceProvider = context.RequestServices ?? applicationServices;
            return serviceProvider;
        }

        private static Func<T, IRabbitContext, IServiceProvider, Task> Compile<T>(MethodInfo methodinfo, IReadOnlyList<ParameterInfo> parameters)
        {
            var middleware = typeof(T);

            var httpContextArg = Expression.Parameter(typeof(IRabbitContext), "rabbitContext");
            var providerArg = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            var instanceArg = Expression.Parameter(middleware, "middleware");

            var methodArguments = new Expression[parameters.Count];
            methodArguments[0] = httpContextArg;
            for (var i = 1; i < parameters.Count; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    throw new NotSupportedException($"InvokeDoesNotSupportRefOrOutParams({InvokeMethodName})");
                }

                var parameterTypeExpression = new Expression[]
                {
                    providerArg,
                    Expression.Constant(parameterType, typeof(Type)),
                    Expression.Constant(methodinfo.DeclaringType, typeof(Type))
                };

                var getServiceCall = Expression.Call(GetServiceInfo, parameterTypeExpression);
                methodArguments[i] = Expression.Convert(getServiceCall, parameterType);
            }

            Expression middlewareInstanceArg = instanceArg;
            if (methodinfo.DeclaringType != typeof(T))
            {
                middlewareInstanceArg = Expression.Convert(middlewareInstanceArg, methodinfo.DeclaringType);
            }

            var body = Expression.Call(middlewareInstanceArg, methodinfo, methodArguments);

            var lambda =
                Expression.Lambda<Func<T, IRabbitContext, IServiceProvider, Task>>(body, instanceArg, httpContextArg,
                    providerArg);

            return lambda.Compile();
        }

        private static object GetService(IServiceProvider sp, Type type, Type middleware)
        {
            var service = sp.GetService(type);
            if (service == null)
            {
                throw new InvalidOperationException($"InvokeMiddlewareNoService({type}, {middleware})");
            }

            return service;
        }

        #endregion Private Method
    }
}