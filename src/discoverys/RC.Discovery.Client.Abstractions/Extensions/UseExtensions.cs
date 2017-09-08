using System;
using System.Threading.Tasks;

namespace RC.Discovery.Client.Abstractions.Extensions
{
    public static class UseExtensions
    {
        public static IRabbitApplicationBuilder Use(this IRabbitApplicationBuilder app, Func<RabbitContext, Func<Task>, Task> middleware)
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