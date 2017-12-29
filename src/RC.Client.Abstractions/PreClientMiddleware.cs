using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.Abstractions.Features;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class PreClientMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public PreClientMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            context.Features.Set<IServiceRequestFeature>(new ServiceRequestFeature(context.Features.Get<IRequestFeature>()));
            await _next(context);
        }
    }
}