using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Internal
{
    public interface IRequestMessageBuilder
    {
        Task BuildAsync(RequestMessageBuilderContext context);
    }

    public abstract class RequestMessageBuilder : IRequestMessageBuilder
    {
        #region Implementation of IRequestMessageBuilder

        public virtual Task BuildAsync(RequestMessageBuilderContext context)
        {
            Build(context);
            return Task.CompletedTask;
        }

        public virtual void Build(RequestMessageBuilderContext context)
        {
        }

        #endregion Implementation of IRequestMessageBuilder
    }
}