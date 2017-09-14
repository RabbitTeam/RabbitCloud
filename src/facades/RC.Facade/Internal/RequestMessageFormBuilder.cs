using Rabbit.Cloud.Facade.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageFormBuilder : IRequestMessageBuilder
    {
        #region Implementation of IRequestMessageBuilder

        public void Build(RequestMessageBuilderContext context)
        {
            if (context.RequestMessage.Content != null)
                return;

            var method = context.Method;
            var request = context.RequestMessage;

            var parameters = GetBodyParameters(request.Method, method).ToArray();
            var isAppendPrefix = parameters.Length > 1;

            var dictionary = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                AppendParameter(isAppendPrefix ? parameter.Key.Name : string.Empty, context.GetArgument(parameter.Value.Name), dictionary);
            }

            request.Content = new FormUrlEncodedContent(dictionary);
        }

        #endregion Implementation of IRequestMessageBuilder

        #region Private Method

        private static IEnumerable<KeyValuePair<ToFormAttribute, ParameterInfo>> GetBodyParameters(HttpMethod httpMethod, MethodBase method)
        {
            foreach (var parameterInfo in method.GetParameters())
            {
                var metadatas = parameterInfo.GetCustomAttributes(false).OfType<IBuilderMetadata>().ToArray();
                var toFormAttribute = metadatas.OfType<ToFormAttribute>().LastOrDefault();

                if (toFormAttribute != null)
                    yield return new KeyValuePair<ToFormAttribute, ParameterInfo>(toFormAttribute, parameterInfo);

                if (metadatas.Any())
                    continue;

                if (httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Head)
                    continue;

                toFormAttribute = new ToFormAttribute(parameterInfo.Name);
                yield return new KeyValuePair<ToFormAttribute, ParameterInfo>(toFormAttribute, parameterInfo);
            }
        }

        private static void AppendParameter(string prefix, object instance, IDictionary<string, string> dictionary)
        {
            var type = instance.GetType();

            var properties = type.GetProperties().Where(i => i.GetMethod != null);

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(instance);

                var key = string.IsNullOrEmpty(prefix) ? propertyInfo.Name : $"{prefix}.{propertyInfo.Name}";
                if (value is IConvertible convertible)
                {
                    dictionary[key] = convertible.ToString(CultureInfo.InvariantCulture);
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