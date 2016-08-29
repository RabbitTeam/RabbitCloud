using RabbitCloud.Rpc.Default.Utils;

namespace RabbitCloud.Rpc.Default.Service.Message
{
    public class RequestMessage : RpcMessage
    {
        public string MethodName { get; set; }

        /// <summary>
        /// 服务参数。
        /// </summary>
        public object[] Arguments { get; set; }

        public static RequestMessage Create(string methodName, object[] arguments)
        {
            return new RequestMessage
            {
                Id = MessageIdGenerator.GeneratorId(),
                Arguments = arguments,
                MethodName = methodName
            };
        }
    }
}