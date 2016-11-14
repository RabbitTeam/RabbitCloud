using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    public class DefaultRequest : IRequest
    {
        private readonly IDictionary<string, string> _parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #region Implementation of IRequest

        /// <summary>
        /// 请求Id。
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// 接口名称。
        /// </summary>
        public string InterfaceName { get; set; }

        /// <summary>
        /// 方法名称。
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 参数类型。
        /// </summary>
        public string[] ParamtersType { get; set; }

        /// <summary>
        /// 服务参数。
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 获取请求参数。
        /// </summary>
        /// <returns>键值对应数组。</returns>
        public NameValueCollection GetParameters()
        {
            var parameters = new NameValueCollection();
            foreach (var parameter in _parameters)
                parameters.Add(parameter.Key, parameter.Value);
            return parameters;
        }

        /// <summary>
        /// 设置一个请求参数。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值。</param>
        public void SetParameter(string name, string value)
        {
            _parameters[name] = value;
        }

        /// <summary>
        /// 获取一个请求参数。
        /// </summary>
        /// <param name="name">参数名称。</param>
        /// <returns>参数值。</returns>
        public string GetParameter(string name)
        {
            return _parameters[name];
        }

        #endregion Implementation of IRequest
    }
}