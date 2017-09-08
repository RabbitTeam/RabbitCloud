using Rabbit.Cloud.Discovery.Client.Builder;
using Rabbit.Cloud.Facade.Middlewares;
using RC.Discovery.Client.Abstractions;
using RC.Discovery.Client.Abstractions.Extensions;

namespace Rabbit.Cloud.Facade.Builder
{
    public static class FacadeApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseFacade(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<FacadeMiddleware>()
                .UseRabbitClient();
        }
    }
}