using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    public static class MapServiceExtensions
    {
        public static IRpcApplicationBuilder MapServiceId(this IRpcApplicationBuilder app, string serviceId,
            Func<RpcContext, Func<Task>, Task> middleware)
        {
            return app.MapWhen(context => context.Request.ServiceId == serviceId, branchBuilder =>
            {
                branchBuilder.Use(middleware);
            });
        }
    }
}