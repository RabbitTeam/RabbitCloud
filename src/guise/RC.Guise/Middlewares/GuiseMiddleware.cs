using Rabbit.Cloud.Client.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Guise.Middlewares
{
    public class GuiseMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public GuiseMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            await _next(context);
        }
    }
}