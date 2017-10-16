using System;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public static class MapExtensions
    {
        public static IRabbitApplicationBuilder Map(this IRabbitApplicationBuilder app, string pathMatch, Action<IRabbitApplicationBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (!string.IsNullOrWhiteSpace(pathMatch) && pathMatch.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The path must not end with a '/'", nameof(pathMatch));
            }

            // create branch
            var branchBuilder = app.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();

            var options = new MapOptions
            {
                Branch = branch,
                PathMatch = pathMatch,
            };
            return app.Use(next => new MapMiddleware(next, options).Invoke);
        }
    }
}