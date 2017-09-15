using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Abstractions.ModelBinding;
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

            var routeTemplate = serviceDescriptor.AttributeRouteInfo.Template;
            var placeholders = GetPlaceholders(routeTemplate);

            var routeUrl = BuildPathAndQuery(routeTemplate,
                serviceDescriptor.RouteValues
                    .Concat(placeholders.ToDictionary(i => i, i => requestContext.GetArgument(i)?.ToString()))
                    .ToDictionary(i => i.Key, i => i.Value));

            requestMessage.Method = serviceDescriptor.HttpMethod;

            var querys = new List<KeyValuePair<string, string>>();
            var headers = new List<KeyValuePair<string, string>>();
            var forms = new List<KeyValuePair<string, string>>();

            foreach (var parameterDescriptor in serviceDescriptor.Parameters)
            {
                var bindingInfo = parameterDescriptor.BindingInfo;
                var source = bindingInfo.BindingSource;

                var key = bindingInfo.BinderModelName ?? parameterDescriptor.Name;
                var value = bindingInfo.DefaultValue ?? requestContext.GetArgument(parameterDescriptor.Name);

                if (source == BindingSource.Query)
                {
                    querys.Add(new KeyValuePair<string, string>(key, value?.ToString()));
                }
                else if (source == BindingSource.Header)
                {
                    headers.Add(new KeyValuePair<string, string>(key, value?.ToString()));
                }
                else if (source == BindingSource.Form)
                {
                    AppendParameter(bindingInfo.BinderModelName ?? string.Empty, value, forms);
                }
                else if (source == BindingSource.Body)
                {
                    var inputFormatterWriteContext = new InputFormatterWriteContext(rabbitContext, parameterDescriptor.ParameterType, value)
                    {
                        ContentType = "application/json"
                    };
                    var formatter = _facadeOptions.InputFormatters.FirstOrDefault(i => i.CanWriteResult(inputFormatterWriteContext));

                    await formatter.WriteAsync(inputFormatterWriteContext);
                }
            }

            if (requestMessage.Content == null)
            {
                requestMessage.Content = new FormUrlEncodedContent(forms);
            }

            var baseUrl = serviceDescriptor.BaseUrl;
            if (!baseUrl.StartsWith("http"))
                baseUrl = "http://" + baseUrl;

            requestMessage.RequestUri = new Uri(new Uri(baseUrl), routeUrl);
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