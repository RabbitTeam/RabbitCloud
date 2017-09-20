using Rabbit.Cloud.Abstractions;
using System.Net.Http;

namespace Rabbit.Cloud.Internal
{
    public class DefaultRabbitResponse : RabbitResponse
    {
        public DefaultRabbitResponse(RabbitContext rabbitContext)
        {
            RabbitContext = rabbitContext;
        }

        #region Overrides of RabbitResponse

        public override RabbitContext RabbitContext { get; }
        public override HttpResponseMessage ResponseMessage { get; set; }

        #endregion Overrides of RabbitResponse
    }
}