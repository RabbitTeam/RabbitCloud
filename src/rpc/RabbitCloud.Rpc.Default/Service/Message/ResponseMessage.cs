namespace RabbitCloud.Rpc.Default.Service.Message
{
    public class ResponseMessage : RpcMessage
    {
        /// <summary>
        /// 异常消息。
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// 结果内容。
        /// </summary>
        public object Result { get; set; }

        public static ResponseMessage Create(RequestMessage requestMessage, object result, string exceptionMessage)
        {
            return new ResponseMessage
            {
                Id = requestMessage.Id,
                ExceptionMessage = exceptionMessage,
                Result = result
            };
        }
    }
}