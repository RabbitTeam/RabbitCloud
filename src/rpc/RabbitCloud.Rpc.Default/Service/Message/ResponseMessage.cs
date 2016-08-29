using System;

namespace RabbitCloud.Rpc.Default.Service.Message
{
    public class ResponseMessage : RpcMessage
    {
        /// <summary>
        /// 异常信息。
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 结果内容。
        /// </summary>
        public object Result { get; set; }

        public static ResponseMessage Create(RequestMessage requestMessage, object result, Exception exception)
        {
            return new ResponseMessage
            {
                Id = requestMessage.Id,
                Exception = exception,
                Result = result
            };
        }
    }
}