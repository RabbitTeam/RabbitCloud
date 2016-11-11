using System;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    public static class MapWhenExtensions
    {
        public static IRpcApplicationBuilder MapWhen(this IRpcApplicationBuilder app, Func<RpcContext, bool> predicate,
            Action<IRpcApplicationBuilder> configuration)
        {
            var branchBuilder = app.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();

            return app.Use(async (context, next) =>
            {
                if (predicate(context))
                    await branch.Invoke(context);
                else
                    await next();
            });
        }
    }
}