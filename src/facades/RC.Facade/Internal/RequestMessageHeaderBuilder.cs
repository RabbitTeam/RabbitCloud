using Rabbit.Cloud.Facade.Abstractions;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageHeaderBuilder : RequestMessageBuilder
    {
        #region Overrides of RequestMessageBuilder

        public override void Build(RequestMessageBuilderContext context)
        {
            var method = context.Method;

            var interfaceHeaders = method.DeclaringType.GetCustomAttributes<ToHeaderAttribute>();
            var methodHeaders = method.GetCustomAttributes<ToHeaderAttribute>();

            var parameterHeaders = method.GetParameters().ToDictionary(i => i, i => i.GetCustomAttributes().OfType<ToHeaderAttribute>().LastOrDefault()).Where(i => i.Value != null).Select(
                i =>
                {
                    var toHeaderAttribute = i.Value;
                    if (string.IsNullOrEmpty(toHeaderAttribute.Name))
                        toHeaderAttribute.Name = i.Key.Name;
                    toHeaderAttribute.Value = context.GetArgument(i.Key.Name)?.ToString();

                    return toHeaderAttribute;
                });

            foreach (var toHeaderAttribute in interfaceHeaders.Concat(methodHeaders).Concat(parameterHeaders).Where(i => !string.IsNullOrEmpty(i.Name)))
            {
                context.RequestMessage.Headers.Add(toHeaderAttribute.Name, toHeaderAttribute.Value);
            }
        }

        #endregion Overrides of RequestMessageBuilder
    }
}