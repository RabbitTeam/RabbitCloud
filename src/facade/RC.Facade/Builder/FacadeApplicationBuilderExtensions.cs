using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Extensions;
using Rabbit.Cloud.Facade.Middlewares;

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