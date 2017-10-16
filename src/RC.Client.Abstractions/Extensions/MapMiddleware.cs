using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public class MapMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly MapOptions _options;

        /// <summary>
        /// Creates a new instace of <see cref="MapMiddleware"/>.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">The middleware options.</param>
        public MapMiddleware(RabbitRequestDelegate next, MapOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="RabbitContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public async Task Invoke(RabbitContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var path = context.Request.RequestUri.AbsolutePath;

            if (path.StartsWith(_options.PathMatch, StringComparison.OrdinalIgnoreCase))
            {
                await _options.Branch(context);
            }
            else
            {
                await _next(context);
            }
        }
    }
}