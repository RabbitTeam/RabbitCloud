using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade.Internal
{
    internal class FacadeBuilder : IFacadeBuilder
    {
        public FacadeBuilder(IRabbitBuilder rabbitBuilder)
        {
            RabbitBuilder = rabbitBuilder;
        }

        #region Implementation of IFacadeBuilder

        public IRabbitBuilder RabbitBuilder { get; }

        #endregion Implementation of IFacadeBuilder
    }
}