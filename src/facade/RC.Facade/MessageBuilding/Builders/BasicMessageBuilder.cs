using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Utilities.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.MessageBuilding.Builders
{
    public class BasicMessageBuilder : IMessageBuilder
    {
        Task IMessageBuilder.BuildAsync(MessageBuilderContext context)
        {
            Build(context);
            return Task.CompletedTask;
        }

        protected virtual void Build(MessageBuilderContext context)
        {
            var collection = GetCollection(context);

            if (collection == null)
                return;

            var values = MessageBuilderUtil.GetValues(context.ParameterDescriptor, GetValue(context));
            foreach (var item in values)
            {
                collection.Add(new KeyValuePair<string, string>(item.Key, item.Value));
            }
        }

        private static ICollection<KeyValuePair<string, string>> GetCollection(MessageBuilderContext context)
        {
            var parameterDescriptor = context.ParameterDescriptor;
            var builderTarget = parameterDescriptor.BuildingInfo.BuildingTarget;

            switch (builderTarget)
            {
                case ToQueryAttribute _:
                    return context.Querys;

                case ToHeaderAttribute _:
                    return context.Headers;

                case ToFormAttribute _:
                    return context.Forms;
            }

            return null;
        }

        private static object GetValue(MessageBuilderContext context)
        {
            var parameterDescriptor = context.ParameterDescriptor;
            var builderTarget = parameterDescriptor.BuildingInfo.BuildingTarget;
            if (builderTarget.BuildingTarget.Id == BuildingTarget.Custom.Id)
            {
                switch (builderTarget)
                {
                    case ToQueryAttribute attribute:
                        return attribute.Value;

                    case ToHeaderAttribute attribute:
                        return attribute.Value;

                    case ToFormAttribute attribute:
                        return attribute.Value;
                }
            }
            return context.ServiceRequestContext.GetArgument(parameterDescriptor.Name);
        }
    }
}