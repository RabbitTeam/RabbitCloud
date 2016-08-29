using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Utils;

namespace RabbitCloud.Rpc.Default.Service.Message
{
    public class RequestMessage : RpcMessage
    {
        public Invocation Invocation { get; set; }

        public static RequestMessage Create(Invocation invocation)
        {
            return new RequestMessage
            {
                Id = MessageIdGenerator.GeneratorId(),
                Invocation = invocation
            };
        }
    }
}