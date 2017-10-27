using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public static class UseMiddlewareExtensions
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";

        private static readonly MethodInfo GetServiceInfo = typeof(UseMiddlewareExtensions).GetMethod(nameof(GetService), BindingFlags.NonPublic | BindingFlags.Static);

        public static IRabbitApplicationBuilder<TContext> UseMiddleware<TContext, TMiddleware>(this IRabbitApplicationBuilder<TContext> app, params object[] args)
        {
            return app.UseMiddleware(typeof(TMiddleware), args);
        }

        public static IRabbitApplicationBuilder<TContext> UseMiddleware<TContext>(this IRabbitApplicationBuilder<TContext> app, Type middleware, params object[] args)
        {
            if (typeof(IRabbitMiddleware<TContext>).GetTypeInfo().IsAssignableFrom(middleware.GetTypeInfo()))
            {
                if (args.Length > 0)
                    throw new NotSupportedException($"{typeof(IRabbitMiddleware<TContext>)} not supported args.");

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
                if (parameters.Length == 0 || parameters[0].ParameterType != typeof(TContext))
                {
                    throw new InvalidOperationException($"UseMiddlewareNoParameters({InvokeMethodName}, {InvokeAsyncMethodName}, {typeof(TContext).Name})");
                }

                var ctorArgs = new object[args.Length + 1];
                ctorArgs[0] = next;
                Array.Copy(args, 0, ctorArgs, 1, args.Length);
                var instance = ActivatorUtilities.CreateInstance(app.ApplicationServices, middleware, ctorArgs);
                if (parameters.Length == 1)
                {
                    return (RabbitRequestDelegate<TContext>)methodinfo.CreateDelegate(typeof(RabbitRequestDelegate<TContext>), instance);
                }
                var factory = Compile<TContext, object>(methodinfo, parameters);
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

        private static IRabbitApplicationBuilder<TContext> UseMiddlewareInterface<TContext>(IRabbitApplicationBuilder<TContext> app, Type middlewareType)
        {
            return app.Use(next =>
            {
                return async context =>
                {
                    var middleware = (IRabbitMiddleware<TContext>)GetServices(app.ApplicationServices, context).GetService(middlewareType);
                    if (middleware == null)
                    {
                        throw new InvalidOperationException($"UseMiddlewareUnableToCreateMiddleware {middlewareType}");
                    }

                    await middleware.InvokeAsync(context, next);
                };
            });
        }

        private static IServiceProvider GetServices<TContext>(IServiceProvider applicationServices, TContext context)
        {
            var serviceProvider = applicationServices;
            if (context is IRabbitContext rabbitContext)
                serviceProvider = rabbitContext.RequestServices ?? applicationServices;
            return serviceProvider;
        }

        private static Func<T, TContext, IServiceProvider, Task> Compile<TContext, T>(MethodInfo methodinfo, IReadOnlyList<ParameterInfo> parameters)
        {
            var middleware = typeof(T);

            var httpContextArg = Expression.Parameter(typeof(TContext), "rabbitContext");
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
                Expression.Lambda<Func<T, TContext, IServiceProvider, Task>>(body, instanceArg, httpContextArg,
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