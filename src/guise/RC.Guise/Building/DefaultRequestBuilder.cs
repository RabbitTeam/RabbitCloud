using Rabbit.Cloud.Client.Http;
using Rabbit.Cloud.Guise.Abstractions.Building;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Guise.Building
{
    public class DefaultRequestBuilder : IRequestBuilder
    {
        public Task BuildAsync(BuildingContext buildingContext)
        {
            var serviceDescriptor = buildingContext.ServiceDescriptor;
            var rabbitContext = buildingContext.RabbitContext;
            var arguments = buildingContext.Arguments;

            var request = (HttpRabbitRequest)rabbitContext.Request;
            request.Method = HttpMethodExtensions.GetHttpMethod(serviceDescriptor.HttpMethod, HttpMethod.Get);

            var values = new List<KeyValuePair<string, string>>();
            foreach (var parameterDescriptor in serviceDescriptor.Parameters)
            {
                var value = arguments[parameterDescriptor.Name];
                values.AddRange(MessageBuilderUtil.GetValues(parameterDescriptor, value));
            }

            var route = serviceDescriptor.ServiceRouteInfo.Template;
            var routeKeys = GetPlaceholders(route).ToArray();
            var routeData = GetRouteData(routeKeys, values);

            request.RequestUri = new Uri(BuildPathAndQuery(route, routeData));

            return Task.CompletedTask;
        }

        private static IDictionary<string, string> GetRouteData(IEnumerable<string> routeKeys, IEnumerable<KeyValuePair<string, string>> values)
        {
            return routeKeys.ToDictionary(i => i, i =>
            {
                var items = values.Where(z => string.Equals(z.Key, i, StringComparison.OrdinalIgnoreCase)).Select(z => z.Value);
                return string.Join(",", items);
            });
        }

        private static string BuildPathAndQuery(string pathAndQuery, IDictionary<string, string> values)
        {
            if (values == null || !values.Any())
                return pathAndQuery;

            var builder = new StringBuilder(pathAndQuery);
            foreach (var value in values)
            {
                builder.Replace($"{{{value.Key}}}", value.Value);
            }

            return builder.ToString();
        }

        private static IEnumerable<string> GetPlaceholders(string value)
        {
            foreach (Match match in Regex.Matches(value, "{(\\w+)}"))
            {
                var key = match.Groups[1].Value;
                yield return key;
            }
        }
    }
}