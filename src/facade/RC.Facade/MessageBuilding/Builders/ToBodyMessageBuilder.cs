using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Utilities.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.MessageBuilding.Builders
{
    public class ToBodyMessageBuilder : MessageBuilder<ToBodyAttribute>
    {
        private readonly FacadeOptions _facadeOptions;

        public ToBodyMessageBuilder(IOptions<FacadeOptions> facadeOptions)
        {
            _facadeOptions = facadeOptions.Value;
        }

        #region Overrides of MessageBuilder<ToBodyAttribute>

        protected override Task BuildAsync(MessageBuilderContext context)
        {
            if (context.ServiceRequestContext.RabbitContext.Request.Body.Length > 0)
                return Task.CompletedTask;

            var parameterDescriptor = context.ParameterDescriptor;
            var inputFormatterWriteContext = new InputFormatterWriteContext(context.ServiceRequestContext.RabbitContext, parameterDescriptor.ParameterType,
                context.ServiceRequestContext.GetArgument(parameterDescriptor.Name))
            {
                ContentType = BuilderTarget.Formatter ?? "application/json"
            };
            var formatter =
                _facadeOptions.InputFormatters.FirstOrDefault(i => i.CanWriteResult(inputFormatterWriteContext));

            return formatter.WriteAsync(inputFormatterWriteContext);
        }

        #endregion Overrides of MessageBuilder<ToBodyAttribute>
    }
}