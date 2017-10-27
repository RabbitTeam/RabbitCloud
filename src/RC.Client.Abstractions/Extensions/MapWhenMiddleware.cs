using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions.Extensions
{
    public class MapWhenMiddleware<TContext>
    {
        private readonly RabbitRequestDelegate<TContext> _next;
        private readonly MapWhenOptions<TContext> _options;

        public MapWhenMiddleware(RabbitRequestDelegate<TContext> next, MapWhenOptions<TContext> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Invoke(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_options.Predicate(context))
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