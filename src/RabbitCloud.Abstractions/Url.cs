using System;
using System.Collections.Generic;

namespace RabbitCloud.Abstractions
{
    public class Url : Uri
    {
        private IDictionary<string, string> _parameters;

        public IDictionary<string, string> Parameters
        {
            get
            {
                if (_parameters != null)
                    return _parameters;
                _parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var temp = Query.Split('&');
                foreach (var item in temp)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    var values = item.Split('=');
                    _parameters[values[0]] = values.Length == 1 ? string.Empty : values[1];
                }
                return _parameters;
            }
        }

        /// <summary>用指定的 URI 初始化 <see cref="T:System.Uri" /> 类的新实例。</summary>
        /// <param name="uriString">一个 URI。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="uriString" /> 为 null。</exception>
        /// <exception cref="T:System.UriFormatException">在 .NET for Windows Store apps 或 可移植类库, ，捕获该基类异常， <see cref="T:System.FormatException" />, 、 相反。<paramref name="uriString" /> 为空。- 或 - <paramref name="uriString" /> 中指定的方案形式不正确。请参阅 <see cref="M:System.Uri.CheckSchemeName(System.String)" />。- 或 - <paramref name="uriString" /> 包含太多斜杠。- 或 - <paramref name="uriString" /> 中指定的密码无效。- 或 - <paramref name="uriString" /> 中指定的主机名无效。- 或 - <paramref name="uriString" /> 中指定的文件名无效。- 或 - <paramref name="uriString" /> 中指定的用户名无效。- 或 - <paramref name="uriString" /> 中指定的主机名或证书颁发机构名不能以反斜杠结尾。- 或 - <paramref name="uriString" /> 中指定的端口号无效或无法分析。- 或 - 长度 <paramref name="uriString" /> 超过 65519 个字符。- 或 - <paramref name="uriString" /> 中指定的方案的长度超过 1023 个字符。- 或 - <paramref name="uriString" /> 中存在无效的字符序列。- 或 - <paramref name="uriString" /> 中指定的 MS-DOS 路径必须以 c:\\ 开头。</exception>
        public Url(string uriString) : base(uriString)
        {
        }

        /// <summary>用指定的 URI 初始化 <see cref="T:System.Uri" /> 类的新实例。此构造函数允许指定 URI 字符串是相对 URI、绝对 URI 还是不确定。</summary>
        /// <param name="uriString">标识将由 <see cref="T:System.Uri" /> 实例表示的资源的字符串。</param>
        /// <param name="uriKind">指定 URI 字符串是相对 URI、绝对 URI 还是不确定。</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="uriKind" /> 无效。</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="uriString" /> 为 null。</exception>
        /// <exception cref="T:System.UriFormatException">在 .NET for Windows Store apps 或 可移植类库, ，捕获该基类异常， <see cref="T:System.FormatException" />, 、 相反。<paramref name="uriString" /> 包含相对 URI，而 <paramref name="uriKind" /> 为 <see cref="F:System.UriKind.Absolute" />。或<paramref name="uriString" /> 包含绝对 URI，而 <paramref name="uriKind" /> 为 <see cref="F:System.UriKind.Relative" />。或<paramref name="uriString" /> 为空。- 或 - <paramref name="uriString" /> 中指定的方案形式不正确。请参阅 <see cref="M:System.Uri.CheckSchemeName(System.String)" />。- 或 - <paramref name="uriString" /> 包含太多斜杠。- 或 - <paramref name="uriString" /> 中指定的密码无效。- 或 - <paramref name="uriString" /> 中指定的主机名无效。- 或 - <paramref name="uriString" /> 中指定的文件名无效。- 或 - <paramref name="uriString" /> 中指定的用户名无效。- 或 - <paramref name="uriString" /> 中指定的主机名或证书颁发机构名不能以反斜杠结尾。- 或 - <paramref name="uriString" /> 中指定的端口号无效或无法分析。- 或 - 长度 <paramref name="uriString" /> 超过 65519 个字符。- 或 - <paramref name="uriString" /> 中指定的方案的长度超过 1023 个字符。- 或 - <paramref name="uriString" /> 中存在无效的字符序列。- 或 - <paramref name="uriString" /> 中指定的 MS-DOS 路径必须以 c:\\ 开头。</exception>
        public Url(string uriString, UriKind uriKind) : base(uriString, uriKind)
        {
        }

        /// <summary>根据指定的基 URI 和相对 URI 字符串，初始化 <see cref="T:System.Uri" /> 类的新实例。</summary>
        /// <param name="baseUri">基 URI。</param>
        /// <param name="relativeUri">要添加到基 URI 的相对 URI。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="baseUri" /> 为 null。</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="baseUri" /> 不是绝对的 <see cref="T:System.Uri" /> 实例。</exception>
        /// <exception cref="T:System.UriFormatException">在 .NET for Windows Store apps 或 可移植类库, ，捕获该基类异常， <see cref="T:System.FormatException" />, 、 相反。<paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 是空的或只包含空格。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的方案无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合所形成的 URI 包含太多的斜杠。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的密码无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的主机名无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的文件名无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的用户名无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的主机名或证书颁发机构名不能以反斜杠结尾。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的端口号无效或无法分析。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 的长度超过 65519 个字符。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的方案的长度超过 1023 个字符。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中存在无效的字符序列。- 或 - <paramref name="uriString" /> 中指定的 MS-DOS 路径必须以 c:\\ 开头。</exception>
        public Url(Uri baseUri, string relativeUri) : base(baseUri, relativeUri)
        {
        }

        /// <summary>根据指定的基 <see cref="T:System.Uri" /> 实例和相对 <see cref="T:System.Uri" /> 实例的组合，初始化 <see cref="T:System.Uri" /> 类的新实例。</summary>
        /// <param name="baseUri">作为新 <see cref="T:System.Uri" /> 实例的基的绝对 <see cref="T:System.Uri" />。</param>
        /// <param name="relativeUri">与 <see cref="T:System.Uri" /> 组合的相对 <paramref name="baseUri" /> 实例。</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="baseUri" /> 不是绝对的 <see cref="T:System.Uri" /> 实例。</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="baseUri" /> 为 null。</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="baseUri" /> 不是绝对的 <see cref="T:System.Uri" /> 实例。</exception>
        /// <exception cref="T:System.UriFormatException">在 .NET for Windows Store apps 或 可移植类库, ，捕获该基类异常， <see cref="T:System.FormatException" />, 、 相反。<paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 是空的或只包含空格。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的方案无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合所形成的 URI 包含太多的斜杠。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的密码无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的主机名无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的文件名无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的用户名无效。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的主机名或证书颁发机构名不能以反斜杠结尾。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的端口号无效或无法分析。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 的长度超过 65519 个字符。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中指定的方案的长度超过 1023 个字符。- 或 - <paramref name="baseUri" /> 和 <paramref name="relativeUri" /> 组合形成的 URI 中存在无效的字符序列。- 或 - <paramref name="uriString" /> 中指定的 MS-DOS 路径必须以 c:\\ 开头。</exception>
        public Url(Uri baseUri, Uri relativeUri) : base(baseUri, relativeUri)
        {
        }
    }
}