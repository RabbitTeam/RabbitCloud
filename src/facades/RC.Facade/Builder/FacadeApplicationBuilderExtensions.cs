using Rabbit.Cloud.Facade.Middlewares;
using RC.Abstractions;
using RC.Discovery.Client.Abstractions.Extensions;

namespace Rabbit.Cloud.Facade.Builder
{
    public static class FacadeApplicationBuilderExtensions
    {
        public static IRabbitApplicationBuilder UseFacade(this IRabbitApplicationBuilder app)
        {
            return app
                .UseMiddleware<FacadeMiddleware>();
        }
    }
}