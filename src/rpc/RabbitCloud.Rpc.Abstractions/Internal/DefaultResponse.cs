using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace RabbitCloud.Rpc.Abstractions.Internal
{
    public class DefaultResponse : IResponse
    {
        private readonly IDictionary<string, string> _parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public DefaultResponse()
        {
        }

        public DefaultResponse(IResponse response)
        {
            Result = response.Result;
            Exception = response.Exception;
            var parameters = response.GetParameters();
            _parameters = parameters.AllKeys.ToDictionary(i => i, i => parameters[i]);
            RequestId = response.RequestId;
        }

        #region Implementation of IResponse

        /// <summary>
        /// 请求Id。
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// 响应结果。
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 响应结果中发生的异常。
        /// </summary>
        public Exception Exception { get; set; }

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

        #endregion Implementation of IResponse
    }
}