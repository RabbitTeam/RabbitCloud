using System;

namespace RabbitCloud.Rpc.Abstractions.Extensions
{
    public static class UseWhenExtensions
    {
        public static IRpcApplicationBuilder UseWhen(this IRpcApplicationBuilder app, Func<RpcContext, bool> predicate,
            Action<IRpcApplicationBuilder> configuration)
        {
            var branchBuilder = app.New();
            configuration(branchBuilder);

            return app.Use(main =>
           {
               branchBuilder.Use(async (context, next) =>
               {
                   await main(context);
               });

               var branch = branchBuilder.Build();
               return context => predicate(context) ? branch(context) : main(context);
           });
        }
    }
}