using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    public static class RegisterServiceExtensions
    {
        public static IRpcApplicationBuilder RegisterService(this IRpcApplicationBuilder app,
            string serviceId, Func<object[], Task<object>> invoker)
        {
            return app.UseWhen(context => context.Request.ServiceId == serviceId, c =>
            {
                c.Use(async (context, next) =>
                {
                    var args = (object[])context.Request.Body;
                    context.Response.Body = await invoker(args);
                    await next();
                });
            });
        }

        public static IRpcApplicationBuilder RegisterService<TService>(this IRpcApplicationBuilder app)
        {
            var methods = typeof(TService).GetMethods();
            var service = app.ApplicationServices.GetRequiredService<TService>();

            foreach (var method in methods)
            {
                app.RegisterService(GetServiceId(method), async args =>
                {
                    var result = method.Invoke(service, args);
                    var resultTask = result as Task;
                    if (resultTask == null)
                        return result;
                    await resultTask;
                    return resultTask.GetType().GetProperty("Result").GetValue(resultTask);
                });
            }
            return app;
        }

        private static string GetServiceId(MethodBase method)
        {
            var parameters = string.Join("_", method.GetParameters().Select(i => i.ParameterType.FullName));
            var id = $"{method.DeclaringType.FullName}.{method.Name}";
            if (string.IsNullOrEmpty(parameters))
                return id;
            return id + "_" + parameters;
        }
    }
}