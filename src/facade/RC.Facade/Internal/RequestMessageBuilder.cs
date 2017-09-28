using Microsoft.AspNetCore.WebUtilities;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Abstractions.Utilities.Extensions;
using System;
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
        private readonly IEnumerable<IMessageBuilder> _messageBuilders;

        public RequestMessageBuilder(IEnumerable<IMessageBuilder> messageBuilders)
        {
            _messageBuilders = messageBuilders;
        }

        #region Implementation of IRequestMessageBuilder

        private async Task<MessageBuilderContext> MessageBuildAsync(ServiceRequestContext serviceRequestContext)
        {
            var requestContext = serviceRequestContext;
            var serviceDescriptor = requestContext.ServiceDescriptor;
            var messageBuilderContext = new MessageBuilderContext(requestContext);

            // message build handle
            foreach (var parameterDescriptor in serviceDescriptor.Parameters)
            {
                messageBuilderContext.ParameterDescriptor = parameterDescriptor;
                foreach (var messageBuilder in _messageBuilders)
                {
                    await messageBuilder.BuildAsync(messageBuilderContext);
                }
            }

            return messageBuilderContext;
        }

        private static IDictionary<string, string> GetRouteData(IEnumerable<string> routeKeys, IEnumerable<KeyValuePair<string, string>> values)
        {
            return routeKeys.ToDictionary(i => i, i =>
            {
                var items = values.Where(z => string.Equals(z.Key, i, StringComparison.OrdinalIgnoreCase)).Select(z => z.Value);
                return string.Join(",", items);
            });
        }

        public async Task BuildAsync(RequestMessageBuilderContext context)
        {
            var requestContext = context.ServiceRequestContext;
            var serviceDescriptor = requestContext.ServiceDescriptor;
            var rabbitContext = requestContext.RabbitContext;
            var requestMessage = rabbitContext.Request.RequestMessage;

            // set httpMethod
            requestMessage.Method = serviceDescriptor.HttpMethod;

            var messageBuilderContext = await MessageBuildAsync(requestContext);

            // add headers and body
            var querys = messageBuilderContext.Querys;
            var headers = messageBuilderContext.Headers;
            var forms = messageBuilderContext.Forms;
            foreach (var item in headers)
                requestMessage.Headers.Add(item.Key, item.Value);
            if (requestMessage.Method.HaveBody() && requestMessage.Content == null && forms != null && forms.Any())
                requestMessage.Content = new FormUrlEncodedContent(forms);

            // resolve url
            var routeTemplate = serviceDescriptor.ServiceRouteInfo.Template;
            var routeKeys = GetPlaceholders(routeTemplate).ToArray();
            var routeData = GetRouteData(routeKeys, querys);
            var routeUrl = BuildPathAndQuery(routeTemplate, routeData);

            // remove placeholder to query
            var finallyQuerys = querys.Where(i => !routeKeys.Contains(i.Key, StringComparer.OrdinalIgnoreCase)).ToArray();

            // build url
            var url = routeUrl;
            if (!url.StartsWith("http"))
                url = "http://" + url;

            // add querys
            if (finallyQuerys.Any())
                url = QueryHelpers.AddQueryString(url, finallyQuerys.ToDictionary(i => i.Key, i => i.Value ?? string.Empty));

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

        #endregion Private Method
    }
}