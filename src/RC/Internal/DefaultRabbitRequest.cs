using Rabbit.Cloud.Abstractions;
using System.Net.Http;

namespace Rabbit.Cloud.Internal
{
    public class DefaultRabbitRequest : RabbitRequest
    {
        public DefaultRabbitRequest(RabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
        }

        #region Overrides of RabbitRequest

        public override RabbitContext RabbitContext { get; }
        public override HttpRequestMessage RequestMessage { get; set; }

        #endregion Overrides of RabbitRequest
    }
}