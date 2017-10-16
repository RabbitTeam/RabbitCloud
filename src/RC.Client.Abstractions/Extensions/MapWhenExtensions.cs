using System;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public static class MapWhenExtensions
    {
        public static IRabbitApplicationBuilder MapWhen<T>(this IRabbitApplicationBuilder app, Predicate<T> predicate, Action<IRabbitApplicationBuilder> configuration) where T : IRabbitContext
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
            var options = new MapWhenOptions
            {
                Predicate = context => predicate((T)context),
                Branch = branch,
            };
            return app.Use(next => new MapWhenMiddleware(next, options).Invoke);
        }
    }
}