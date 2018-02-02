using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public class RequestOptionMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly RabbitClientOptions _options;

        public RequestOptionMiddleware(RabbitRequestDelegate next, IOptions<RabbitClientOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var rabbitClientFeature = context.Features.Get<IRabbitClientFeature>();
            if (rabbitClientFeature == null)
                context.Features.Set(rabbitClientFeature = new RabbitClientFeature());

            var serviceId = context.Features.Get<IServiceDiscoveryFeature>()?.ServiceId;

            if (!string.IsNullOrEmpty(serviceId) && !_options.RequestOptionses.TryGetValue(serviceId, out var requestOptions))
                requestOptions = _options.DefaultRequestOptions;
            else
                requestOptions = new ServiceRequestOptions();

            requestOptions.Timeout = GetRequestOption(context, "ReadTimeout", requestOptions.Timeout);
            requestOptions.MaxAutoRetries = GetRequestOption(context, "Retries", requestOptions.MaxAutoRetries);
            requestOptions.MaxAutoRetriesNextServer = GetRequestOption(context, "RetriesNextServer", requestOptions.MaxAutoRetriesNextServer);
            requestOptions.ServiceChooser = GetRequestOption(context, "ServiceChooser", requestOptions.ServiceChooser);

            rabbitClientFeature.RequestOptions = requestOptions;

            await _next(context);
        }

        private static T GetRequestOption<T>(IRabbitContext context, string key, T def)
        {
            var value = GetRequestOption(context, key);
            if (string.IsNullOrEmpty(value))
                return def;

            var type = typeof(T);

            if (type == typeof(string))
            {
                return (T)(object)value;
            }

            if (type == typeof(int))
            {
                int.TryParse(value, out var number);
                if (number <= 0)
                    return def;
                return (T)(object)number;
            }

            if (type == typeof(TimeSpan))
            {
                return (T)(object)TimeUtilities.GetTimeSpanBySimpleOrDefault(value, (TimeSpan)(object)def);
            }

            throw new NotSupportedException(type.ToString());
        }

        private static string GetRequestOption(IRabbitContext context, string key)
        {
            IEnumerable<string> GetRequestOptions()
            {
                var query = context.Request.Query;

                if (query.TryGetValue(key, out var item))
                    yield return item.LastOrDefault();

                var headers = context.Request.Headers;
                if (headers.TryGetValue(key, out item))
                    yield return item.LastOrDefault();
            }

            return GetRequestOptions().FirstOrDefault();
        }
    }
}