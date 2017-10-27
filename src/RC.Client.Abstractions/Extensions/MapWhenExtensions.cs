using System;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public static class MapWhenExtensions
    {
        public static IRabbitApplicationBuilder<TContext> MapWhen<TContext>(this IRabbitApplicationBuilder<TContext> app, Predicate<TContext> predicate, Action<IRabbitApplicationBuilder<TContext>> configuration)
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
            return app.Use(next => new MapWhenMiddleware<TContext>(next, options).Invoke);
        }
    }
}