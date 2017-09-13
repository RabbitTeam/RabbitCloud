using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageContentBuilder : IRequestMessageBuilder
    {
        #region Implementation of IRequestMessageBuilder

        public void Build(RequestMessageBuilderContext context)
        {
            var method = context.Method;
            var parameters = GetBodyParameters(method).ToArray();
            var isAppendPrefix = parameters.Length > 1;

            var request = context.RequestMessage;

            var dictionary = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                AppendParameter(isAppendPrefix ? parameter.Name : string.Empty, context.GetArgument(parameter.Name), dictionary);
            }

            request.Content = new FormUrlEncodedContent(dictionary);
        }

        #endregion Implementation of IRequestMessageBuilder

        #region Private Method

        private static IEnumerable<ParameterInfo> GetBodyParameters(MethodBase method)
        {
            return method.GetParameters().Where(i => i.GetCustomAttributes<FromBodyAttribute>().Any());
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