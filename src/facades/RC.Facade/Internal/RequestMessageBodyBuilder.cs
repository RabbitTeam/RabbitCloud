using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageBodyBuilder : RequestMessageBuilder
    {
        private readonly FacadeOptions _facadeOptions;

        public RequestMessageBodyBuilder(IOptions<FacadeOptions> facadeOptions)
        {
            _facadeOptions = facadeOptions.Value;
        }

        #region Overrides of RequestMessageBuilder

        public override async Task BuildAsync(RequestMessageBuilderContext context)
        {
            if (context.RequestMessage.Content != null)
                return;

            var method = context.Method;
            var parameters = method.GetParameters().ToDictionary(i => i, i => i.GetCustomAttribute<ToBodyAttribute>()).Where(i => i.Value != null).ToArray();

            if (!parameters.Any())
                return;
            if (parameters.Length > 1)
                throw new ArgumentOutOfRangeException(nameof(parameters), nameof(ToBodyAttribute));

            var item = parameters.First();
            var parameter = item.Key;
            var fromBodyAttribute = item.Value;

            var inputFormatterWriteContext = new InputFormatterWriteContext(context.RabbitContext, parameter.ParameterType, context.GetArgument(parameter.Name))
            {
                ContentType = fromBodyAttribute.Formatter ?? "application/json"
            };
            var formatter = _facadeOptions.InputFormatters.FirstOrDefault(i => i.CanWriteResult(inputFormatterWriteContext));

            await formatter.WriteAsync(inputFormatterWriteContext);
        }

        #endregion Overrides of RequestMessageBuilder
    }
}