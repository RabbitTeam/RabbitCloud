using System;

namespace Rabbit.Cloud.Application.Abstractions.Extensions
{
    public static class MapWhenExtensions
    {
        public static IRabbitApplicationBuilder MapWhen<TContext>(this IRabbitApplicationBuilder app, Predicate<TContext> predicate, Action<IRabbitApplicationBuilder> configuration)
            where TContext : IRabbitContext
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // create branch
            var branchBuilder = app.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();

            // put middleware in pipeline
            var options = new MapWhenOptions<TContext>
            {
                Predicate = context => predicate(context),
                Branch = branch,
            };
            return app.Use(next =>
            {
                return async context =>
                {
                    await new MapWhenMiddleware<TContext>(next, options).Invoke((TContext)context);
                };
            });
        }
    }
}