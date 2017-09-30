using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public static class UseExtensions
    {
        public static IRabbitApplicationBuilder<TContext> Use<TContext>(this IRabbitApplicationBuilder<TContext> app, Func<TContext, Func<Task>, Task> middleware)
        {
            return app.Use(next =>
            {
                return context =>
                {
                    Task SimpleNext() => next(context);
                    return middleware(context, SimpleNext);
                };
            });
        }
    }
}