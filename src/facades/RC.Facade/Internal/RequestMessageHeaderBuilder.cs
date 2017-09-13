using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageHeaderBuilder : IRequestMessageBuilder
    {
        #region Implementation of IRequestMessageBuilder

        public void Build(RequestMessageBuilderContext context)
        {
            var method = context.Method;

            var parameters = method.GetParameters().ToDictionary(i => i, i => i.GetCustomAttributes().OfType<FromHeaderAttribute>().LastOrDefault()).Where(i => i.Value != null);

            foreach (var parameter in parameters)
            {
                var parameterInfo = parameter.Key;
                var fromHeaderAttribute = parameter.Value;

                if (string.IsNullOrEmpty(fromHeaderAttribute.Name))
                    fromHeaderAttribute.Name = parameterInfo.Name;

                context.RequestMessage.Headers.Add(fromHeaderAttribute.Name, context.GetArgument(parameterInfo.Name)?.ToString());
            }
        }

        #endregion Implementation of IRequestMessageBuilder
    }
}