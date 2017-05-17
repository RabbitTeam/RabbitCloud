using System.Collections.Generic;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IRequest
    {
        /// <summary>
        /// 请求标识。
        /// </summary>
        long RequestId { get; }

        /// <summary>
        /// 方法键。
        /// </summary>
        MethodKey MethodKey { get; set; }

        /// <summary>
        /// 请求参数。
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// 请求选项。
        /// </summary>
        IReadOnlyDictionary<string, string> Attachments { get; }

        /// <summary>
        /// 设置选项。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="value">值。</param>
        /// <returns>请求。</returns>
        IRequest SetAttachment(string name, string value);
    }

    public class Request : IRequest
    {
        private readonly Dictionary<string, string> _attachments;

        public Request(Dictionary<string, string> attachments = null)
        {
            _attachments = attachments ?? new Dictionary<string, string>();
        }

        #region Implementation of IRequest

        /// <summary>
        /// 请求标识。
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// 方法键。
        /// </summary>
        public MethodKey MethodKey { get; set; }

        /// <summary>
        /// 请求参数。
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 请求选项。
        /// </summary>
        public IReadOnlyDictionary<string, string> Attachments => _attachments;

        /// <summary>
        /// 设置选项。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="value">值。</param>
        /// <returns>请求。</returns>
        public IRequest SetAttachment(string name, string value)
        {
            _attachments[name] = value;
            return this;
        }

        #endregion Implementation of IRequest
    }

    public static class RequestExtensions
    {
        public static string GetAttachment(this IRequest request, string name)
        {
            return request.Attachments[name];
        }
    }
}