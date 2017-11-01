using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public static class UseExtensions
    {
        public static IRabbitApplicationBuilder Use<TContext>(this IRabbitApplicationBuilder app, Func<TContext, Func<Task>, Task> middleware)
            where TContext : IRabbitContext
        {
            return app.Use(next =>
            {
                return context =>
                {
                    Task SimpleNext() => next(context);
                    return middleware((TContext)context, SimpleNext);
                };
            });
        }
    }
}