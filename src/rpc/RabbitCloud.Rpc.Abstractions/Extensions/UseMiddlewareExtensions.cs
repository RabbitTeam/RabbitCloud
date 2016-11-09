using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    public static class UseMiddlewareExtensions
    {
        private const string InvokeMethodName = "Invoke";

        private static readonly MethodInfo GetServiceInfo = typeof(UseMiddlewareExtensions).GetMethod(nameof(GetService), BindingFlags.NonPublic | BindingFlags.Static);

        public static IRpcApplicationBuilder UseMiddleware<TMiddleware>(this IRpcApplicationBuilder app, params object[] args)
        {
            return app.UseMiddleware(typeof(TMiddleware), args);
        }

        public static IRpcApplicationBuilder UseMiddleware(this IRpcApplicationBuilder app, Type middleware, params object[] args)
        {
            var applicationServices = app.ApplicationServices;
            return app.Use(next =>
            {
                var methods = middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                var invokeMethods = methods.Where(m => string.Equals(m.Name, InvokeMethodName, StringComparison.Ordinal)).ToArray();
                if (invokeMethods.Length > 1)
                {
                    throw new InvalidOperationException($"找到多个名称为: {InvokeMethodName} 的方法");
                }

                if (invokeMethods.Length == 0)
                {
                    throw new InvalidOperationException($"没有找到名称为: {InvokeMethodName} 的方法");
                }

                var methodinfo = invokeMethods[0];
                if (!typeof(Task).IsAssignableFrom(methodinfo.ReturnType))
                {
                    throw new InvalidOperationException($"方法的返回类型不是 {nameof(Task)}");
                }

                var parameters = methodinfo.GetParameters();
                if (parameters.Length == 0 || parameters[0].ParameterType != typeof(RpcContext))
                {
                    throw new InvalidOperationException($"方法的第一个参数不是 {nameof(RpcContext)}");
                }

                var ctorArgs = new object[args.Length + 1];
                ctorArgs[0] = next;
                Array.Copy(args, 0, ctorArgs, 1, args.Length);
                var instance = ActivatorUtilities.CreateInstance(app.ApplicationServices, middleware, ctorArgs);
                if (parameters.Length == 1)
                {
                    return (RpcRequestDelegate)methodinfo.CreateDelegate(typeof(RpcRequestDelegate), instance);
                }

                var factory = Compile<object>(methodinfo, parameters);

                return context =>
                {
                    var serviceProvider = context.RequestServices ?? applicationServices;
                    if (serviceProvider == null)
                    {
                        throw new InvalidOperationException("请求服务不可用");
                    }

                    return factory(instance, context, serviceProvider);
                };
            });
        }

        private static Func<T, RpcContext, IServiceProvider, Task> Compile<T>(MethodInfo methodinfo, IReadOnlyList<ParameterInfo> parameters)
        {
            var middleware = typeof(T);

            var rpcContextArg = Expression.Parameter(typeof(RpcContext), "rpcContext");
            var providerArg = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            var instanceArg = Expression.Parameter(middleware, "middleware");

            var methodArguments = new Expression[parameters.Count];
            methodArguments[0] = rpcContextArg;
            for (int i = 1; i < parameters.Count; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    throw new NotSupportedException("参数不支持 ref和out 关键字");
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

            var lambda = Expression.Lambda<Func<T, RpcContext, IServiceProvider, Task>>(body, instanceArg, rpcContextArg, providerArg);

            return lambda.Compile();
        }

        private static object GetService(IServiceProvider sp, Type type)
        {
            var service = sp.GetService(type);
            if (service == null)
            {
                throw new InvalidOperationException($"找不到服务: {type.FullName}");
            }

            return service;
        }
    }
}