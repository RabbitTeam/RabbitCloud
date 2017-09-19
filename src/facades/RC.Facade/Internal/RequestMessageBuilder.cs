using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageBuilder : IRequestMessageBuilder
    {
        private readonly FacadeOptions _facadeOptions;

        public RequestMessageBuilder(IOptions<FacadeOptions> facadeOptions)
        {
            _facadeOptions = facadeOptions.Value;
        }

        #region Implementation of IRequestMessageBuilder

        public async Task BuildAsync(RequestMessageBuilderContext context)
        {
            var requestContext = context.ServiceRequestContext;
            var serviceDescriptor = requestContext.ServiceDescriptor;

            var rabbitContext = requestContext.RabbitContext;

            var requestMessage = rabbitContext.Request.RequestMessage;

            var parameterQuerys = GetValues(context, BuildingTarget.Query);
            var parameterHeaders = GetValues(context, BuildingTarget.Header);
            var forms = GetValues(context, BuildingTarget.Form);
            var body = serviceDescriptor.Parameters.LastOrDefault(i => i.BuildingInfo.BuildingTarget == BuildingTarget.Body);

            var headers = serviceDescriptor.Headers.Concat(parameterHeaders).GroupBy(i => i.Key).Select(i => new KeyValuePair<string, IEnumerable<string>>(i.Key, i.SelectMany(z => z.Value))).ToArray();
            var querys = serviceDescriptor.Querys.Concat(parameterQuerys).GroupBy(i => i.Key).Select(i => new KeyValuePair<string, IEnumerable<string>>(i.Key, i.SelectMany(z => z.Value))).ToArray();

            // add header
            foreach (var header in headers)
                requestMessage.Headers.Add(header.Key, header.Value);

            // resolve url
            var routeTemplate = serviceDescriptor.ServiceRouteInfo.Template;
            var placeholders = GetPlaceholders(routeTemplate).ToArray();
            var routeUrl = BuildPathAndQuery(routeTemplate,
                placeholders.ToDictionary(i => i, i =>
                    {
                        var items = querys.FirstOrDefault(z => string.Equals(z.Key, i, StringComparison.OrdinalIgnoreCase));
                        return items.Value == null ? string.Empty : string.Join(",", items.Value);
                    })
                    .ToDictionary(i => i.Key, i => i.Value));

            // remove placeholder to query
            querys = querys.Where(i => !placeholders.Contains(i.Key, StringComparer.OrdinalIgnoreCase)).ToArray();

            // set httpMethod
            requestMessage.Method = serviceDescriptor.HttpMethod;

            if (body != null)
            {
                var inputFormatterWriteContext = new InputFormatterWriteContext(rabbitContext, body.ParameterType,
                    context.ServiceRequestContext.GetArgument(body.Name))
                {
                    ContentType = "application/json"
                };
                var formatter =
                    _facadeOptions.InputFormatters.FirstOrDefault(i => i.CanWriteResult(inputFormatterWriteContext));

                await formatter.WriteAsync(inputFormatterWriteContext);
            }
            else
            {
                requestMessage.Content = new FormUrlEncodedContent(forms.Select(i => new KeyValuePair<string, string>(i.Key, i.Value == null ? string.Empty : string.Join(",", i.Value))));
            }

            var url = routeUrl;
            if (!url.StartsWith("http"))
                url = "http://" + url;

            url = QueryHelpers.AddQueryString(url,
                querys.ToDictionary(i => i.Key,
                    i => i.Value == null ? string.Empty : string.Join(",", i.Value.Distinct())));

            requestMessage.RequestUri = new Uri(url);
        }

        #endregion Implementation of IRequestMessageBuilder

        #region Private Method

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

        private static IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetValues(RequestMessageBuilderContext context, BuildingTarget buildingTarget)
        {
            var serviceRequestContext = context.ServiceRequestContext;
            var serviceDescriptor = serviceRequestContext.ServiceDescriptor;

            var querys = new List<KeyValuePair<string, string>>();

            foreach (var parameterDescriptor in serviceDescriptor.Parameters.Where(i => i.BuildingInfo.BuildingTarget == buildingTarget))
            {
                var key = parameterDescriptor.BuildingInfo.BuildingModelName ?? parameterDescriptor.Name;
                var argument = serviceRequestContext.GetArgument(parameterDescriptor.Name);

                if (argument is IConvertible convertible)
                    querys.Add(
                        new KeyValuePair<string, string>(key, convertible.ToString(CultureInfo.InvariantCulture)));
                else
                {
                    AppendParameter(key, argument, querys);
                }
            }

            return querys.GroupBy(i => i.Key)
                .Select(i => new KeyValuePair<string, IEnumerable<string>>(i.Key, i.Select(z => z.Value)));
        }

        private static void AppendParameter(string prefix, object instance, ICollection<KeyValuePair<string, string>> dictionary)
        {
            var type = instance.GetType();

            var properties = type.GetProperties().Where(i => i.GetMethod != null);

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(instance);

                var key = string.IsNullOrEmpty(prefix) ? propertyInfo.Name : $"{prefix}.{propertyInfo.Name}";
                if (value is IConvertible convertible)
                {
                    dictionary.Add(new KeyValuePair<string, string>(key, convertible.ToString(CultureInfo.InvariantCulture)));
                }
                else
                {
                    AppendParameter(key, value, dictionary);
                }
            }
        }

        #endregion Private Method
    }
}