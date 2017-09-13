using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class RequestMessageContentBuilder : IRequestMessageBuilder
    {
        private readonly FacadeOptions _facadeOptions;

        public RequestMessageContentBuilder(IOptions<FacadeOptions> facadeOptions)
        {
            _facadeOptions = facadeOptions.Value;
        }

        #region Implementation of IRequestMessageBuilder

        public void Build(RequestMessageBuilderContext context)
        {
            var method = context.Method;
            var parameters = method.GetParameters().ToDictionary(i => i, i => i.GetCustomAttribute<ToBodyAttribute>()).Where(i => i.Value != null).ToArray();

            if (!parameters.Any())
                return;
            if (parameters.Length > 1)
                throw new ArgumentOutOfRangeException(nameof(parameters), "FromBodyAttribute");

            var item = parameters.First();
            var parameter = item.Key;
            var fromBodyAttribute = item.Value;

            var inputFormatterWriteContext = new InputFormatterWriteContext(context.RabbitContext, parameter.ParameterType, context.GetArgument(parameter.Name))
            {
                ContentType = fromBodyAttribute.Formatter ?? "application/json"
            };
            var formatter = _facadeOptions.InputFormatters.FirstOrDefault(i => i.CanWriteResult(inputFormatterWriteContext));

            formatter.WriteAsync(inputFormatterWriteContext).Wait();
        }

        #endregion Implementation of IRequestMessageBuilder
    }
}