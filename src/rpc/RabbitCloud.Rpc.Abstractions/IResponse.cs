using System;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IResponse
    {
        /// <summary>
        /// 请求标识。
        /// </summary>
        long RequestId { get; }

        /// <summary>
        /// 响应值。
        /// </summary>
        object Value { get; }

        /// <summary>
        /// 响应异常。
        /// </summary>
        Exception Exception { get; }
    }

    public static class ResponseExtensions
    {
        public static object GetValue(this IResponse response)
        {
            var exception = response.Exception;
            if (exception != null)
                throw exception;

            return response.Value;
        }
    }

    public class Response : IResponse
    {
        public Response(IRequest request)
        {
            RequestId = request.RequestId;
        }

        public Response()
        {
        }

        #region Implementation of IResponse

        /// <summary>
        /// 请求标识。
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// 响应值。
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 响应异常。
        /// </summary>
        public Exception Exception { get; set; }

        #endregion Implementation of IResponse
    }
}