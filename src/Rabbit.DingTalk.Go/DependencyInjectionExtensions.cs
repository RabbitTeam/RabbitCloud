using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Rabbit.Go;
using Rabbit.Go.Core;
using Rabbit.Go.Interceptors;
using System;
using System.Threading.Tasks;

namespace Rabbit.DingTalk.Go
{
    public class DingTalkGoClientOptions
    {
        public string DefaultAccessToken { get; set; }
    }

    public static class DependencyInjectionExtensions
    {
        internal class DingTalkGoClientRequestInterceptor : IAsyncRequestInterceptor
        {
            private readonly DingTalkGoClientOptions _options;

            public DingTalkGoClientRequestInterceptor(IOptions<DingTalkGoClientOptions> optionsAccessor)
            {
                _options = optionsAccessor.Value;
            }

            #region Implementation of IAsyncRequestInterceptor

            public Task OnRequestExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next)
            {
                var request = context.GoContext.Request;

                const string key = "access_token";
                if (!request.Query.TryGetValue(key, out var values) || values == StringValues.Empty)
                {
                    request.AddQuery(key, _options.DefaultAccessToken);
                }

                return next();
            }

            #endregion Implementation of IAsyncRequestInterceptor
        }

        public static IServiceCollection AddDingTalkGoClient(this IServiceCollection services)
        {
            return services.AddDingTalkGoClient(defaultAccessToken: null);
        }

        public static IServiceCollection AddDingTalkGoClient(this IServiceCollection services, string defaultAccessToken)
        {
            return services
                .AddDingTalkGoClient(options => { options.DefaultAccessToken = defaultAccessToken; });
        }

        public static IServiceCollection AddDingTalkGoClient(this IServiceCollection services, Action<DingTalkGoClientOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return services
                .Configure(configure)
                .AddSingleton<DingTalkGoClientRequestInterceptor>()
                .Configure<GoOptions>(options =>
                {
                    options.Types.Add(typeof(IDingTalkGoClient));
                    options.Interceptors.AddService<DingTalkGoClientRequestInterceptor>();
                })
                .AddSingleton(s => s.GetRequiredService<IGoFactory>().CreateInstance<IDingTalkGoClient>());
        }
    }
}