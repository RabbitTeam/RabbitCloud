using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Utils;

namespace RabbitCloud.Rpc.Default.Service.Message
{
    public class RequestMessage : RpcMessage
    {
        public RpcInvocation Invocation { get; set; }

        public static RequestMessage Create(RpcInvocation invocation)
        {
            return new RequestMessage
            {
                Id = MessageIdGenerator.GeneratorId(),
                Invocation = invocation
            };
        }
    }
}