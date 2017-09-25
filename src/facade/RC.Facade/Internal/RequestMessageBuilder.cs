using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
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

            var parameterQuerys = await GetValuesAsync(context, BuildingTarget.Query);
            var parameterHeaders = await GetValuesAsync(context, BuildingTarget.Header);
            var forms = await GetValuesAsync(context, BuildingTarget.Form);
            var body = serviceDescriptor.Parameters.LastOrDefault(i =>
                i.BuildingInfo.BuildingTarget == BuildingTarget.Body);

            var headers = serviceDescriptor.Headers.Concat(parameterHeaders).GroupBy(i => i.Key).Select(i =>
                new KeyValuePair<string, IEnumerable<string>>(i.Key, i.SelectMany(z => z.Value))).ToArray();
            var querys = serviceDescriptor.Querys.Concat(parameterQuerys).GroupBy(i => i.Key).Select(i =>
                new KeyValuePair<string, IEnumerable<string>>(i.Key, i.SelectMany(z => z.Value))).ToArray();

            // add header
            foreach (var header in headers)
                requestMessage.Headers.Add(header.Key, header.Value);

            // resolve url
            var routeTemplate = serviceDescriptor.ServiceRouteInfo.Template;
            var placeholders = GetPlaceholders(routeTemplate).ToArray();
            var routeUrl = BuildPathAndQuery(routeTemplate,
                placeholders.ToDictionary(i => i, i =>
                    {
                        var items = querys.FirstOrDefault(z =>
                            string.Equals(z.Key, i, StringComparison.OrdinalIgnoreCase));
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
                requestMessage.Content = new FormUrlEncodedContent(forms.Select(i =>
                    new KeyValuePair<string, string>(i.Key,
                        i.Value == null ? string.Empty : string.Join(",", i.Value))));
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

        private static async Task<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> GetValuesAsync(RequestMessageBuilderContext context, BuildingTarget buildingTarget)
        {
            var serviceRequestContext = context.ServiceRequestContext;
            var serviceDescriptor = serviceRequestContext.ServiceDescriptor;

            var items = new List<KeyValuePair<string, string>>();
            foreach (var parameterDescriptor in serviceDescriptor.Parameters.Where(i => i.BuildingInfo.BuildingTarget == buildingTarget))
            {
                var key = parameterDescriptor.BuildingInfo.BuildingModelName ?? parameterDescriptor.Name;
                var argument = serviceRequestContext.GetArgument(parameterDescriptor.Name);

                await AppendParametersAsync(key, argument, items, context);
            }

            return items.GroupBy(i => i.Key).Select(i => new KeyValuePair<string, IEnumerable<string>>(i.Key, i.Select(z => z.Value)));
        }

        private static async Task AppendParametersAsync(string prefix, object value, ICollection<KeyValuePair<string, string>> items, RequestMessageBuilderContext context)
        {
            if (value == null)
                return;
            var valueType = value.GetType();
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Object:
                    if (value is IEnumerable enumerable)
                    {
                        var index = -1;
                        foreach (var item in enumerable)
                        {
                            index++;
                            await AppendParametersAsync($"{prefix}[{index}]", item, items, context);
                        }
                    }
                    else
                    {
                        foreach (var propertyInfo in valueType.GetProperties())
                        {
                            foreach (var builderTypeProviderMetadata in propertyInfo.GetCustomAttributes(false).OfType<IBuilderTypeProviderMetadata>())
                            {
                                var builderType = builderTypeProviderMetadata.BuilderType;
                                if (!typeof(IRequestMessageBuilder).IsAssignableFrom(builderType))
                                    throw new NotSupportedException($"BuilderType '{builderType}' not {nameof(IRequestMessageBuilder)} type.");
                                var requestMessageBuilder =
                                    (IRequestMessageBuilder)Activator.CreateInstance(builderType);
                                await requestMessageBuilder.BuildAsync(context);
                            }
                            await AppendParametersAsync(prefix + "." + propertyInfo.Name, propertyInfo.GetValue(value), items, context);
                        }
                    }
                    break;

                default:
                    items.Add(new KeyValuePair<string, string>(prefix, value.ToString()));
                    break;
            }

            #endregion Private Method
        }
    }
}