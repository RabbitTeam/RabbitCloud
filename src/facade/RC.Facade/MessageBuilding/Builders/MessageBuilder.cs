using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.MessageBuilding.Builders
{
    public abstract class MessageBuilder : IMessageBuilder
    {
        #region Implementation of IMessageBuilder

        Task IMessageBuilder.BuildAsync(MessageBuilderContext context)
        {
            Build(context);
            return BuildAsync(context);
        }

        #endregion Implementation of IMessageBuilder

        protected virtual Task BuildAsync(MessageBuilderContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual void Build(MessageBuilderContext context)
        {
        }
    }

    public abstract class MessageBuilder<T> : IMessageBuilder where T : IBuilderTargetMetadata
    {
        protected T BuilderTarget { get; private set; }

        #region Implementation of IMessageBuilder

        Task IMessageBuilder.BuildAsync(MessageBuilderContext context)
        {
            if (!(context.ParameterDescriptor.BuildingInfo.BuildingTarget is T builderTarget))
                return Task.CompletedTask;

            BuilderTarget = builderTarget;

            Build(context);
            return BuildAsync(context);
        }

        #endregion Implementation of IMessageBuilder

        protected virtual Task BuildAsync(MessageBuilderContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual void Build(MessageBuilderContext context)
        {
        }
    }
}