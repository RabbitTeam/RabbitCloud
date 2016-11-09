using RabbitCloud.Rpc.Abstractions.Features;
using System.Collections.Generic;

namespace RabbitCloud.Rpc.Default.Messages
{
    public class RabbitInvocation
    {
        /// <summary>
        /// 方法参数。
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 请求头。
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 路径。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 查询字符串。
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// 格式、协议。
        /// </summary>
        public string Scheme { get; set; }

        public IRpcRequestFeature ToRequestFeature()
        {
            return new RpcRequestFeature
            {
                Body = new { Arguments },
                Headers = Headers,
                Path = Path,
                PathBase = null,
                QueryString = QueryString,
                Scheme = Scheme
            };
        }
    }
}