using Microsoft.Extensions.DependencyInjection;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Extensions;
using RabbitCloud.Rpc.Abstractions.Features;
using RabbitCloud.Rpc.Default.Messages;

namespace RabbitCloud.Rpc.Default.Extensions
{
    public static class CodecExtensions
    {
        public static IRpcApplicationBuilder UseCodec(this IRpcApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var requestServices = context.RequestServices = scope.ServiceProvider;

                var codec = requestServices.GetRequiredService<ICodec>();

                var requestFeature = context.Features.Get<IRpcRequestFeature>();

                var invocation = (RabbitInvocation)codec.Decode(context.Request.Body, typeof(RabbitInvocation));
                requestFeature.Path = invocation.Path;
                requestFeature.Body = invocation.Arguments;
                requestFeature.Headers = invocation.Headers;
                requestFeature.QueryString = invocation.QueryString;
                requestFeature.Scheme = invocation.Scheme;

                await next.Invoke();
            });
        }
    }
}