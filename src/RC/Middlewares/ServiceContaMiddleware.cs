using Microsoft.Extensions.DependencyInjection;
using RC.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Middlewares
{
    public class ServiceContainerMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ServiceContainerMiddleware(RabbitRequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Invoke(RabbitContext context)
        {
            if (context.RequestServices != null)
                await _next(context);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                context.RequestServices = scope.ServiceProvider;
                await _next(context);
            }
        }
    }
}