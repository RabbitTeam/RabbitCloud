using RabbitCloud.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IRequest
    {
        /// <summary>
        /// 请求标识。
        /// </summary>
        long RequestId { get; }

        /// <summary>
        /// 方法描述符。
        /// </summary>
        MethodDescriptor MethodDescriptor { get; }

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
        private Dictionary<string, string> _attachments;

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
        /// 方法描述符。
        /// </summary>
        public MethodDescriptor MethodDescriptor { get; set; }

        /// <summary>
        /// 请求参数。
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 请求选项。
        /// </summary>
        public IReadOnlyDictionary<string, string> Attachments
        {
            get => _attachments;
            set { _attachments = value.ToDictionary(i => i.Key, i => i.Value); }
        }

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

        public static IRequest SetServiceKey(this IRequest request, ServiceKey serviceKey)
        {
            request.SetAttachment("group", serviceKey.Group);
            request.SetAttachment("name", serviceKey.Name);
            request.SetAttachment("version", serviceKey.Version);

            return request;
        }

        public static ServiceKey GetServiceKey(this IRequest request)
        {
            return new ServiceKey(request.GetAttachment("group"), request.GetAttachment("name"),
                request.GetAttachment("version"));
        }
    }
}