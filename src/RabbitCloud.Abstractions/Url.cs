using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitCloud.Abstractions
{
    public class Url
    {
        #region Constructor

        public Url()
        {
            Parameters = new AttributeDictionary();
        }


        public Url(string scheme, string userName, string password, string host, int port, string path, IDictionary<string, string> parameters)
            : this(scheme, userName, password, host, port, path, new AttributeDictionary(parameters))
        {
        }
        public Url(string scheme, string userName, string password, string host, int port, string path, AttributeDictionary parameters)
        {
            Scheme = scheme;
            UserName = userName;
            Password = password;
            Host = host;
            Port = port;
            Path = path;
            Parameters = parameters;
        }

        #endregion Constructor

        #region Property

        /// <summary>
        /// 格式、协议。
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// 用户名。
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码。
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 主机。
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 路径。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 参数。
        /// </summary>
        public AttributeDictionary Parameters { get; set; }

        #endregion Property

        #region Public Method

        /// <summary>
        /// 克隆出一个新的Url实例。
        /// </summary>
        /// <returns></returns>
        public Url Clone()
        {
            return new Url(Scheme, UserName, Password, Host, Port, Path, Parameters);
        }

        #endregion Public Method

        #region Overrides of Object

        /// <summary>返回表示当前对象的字符串。</summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            var extraValue = string.Empty;

            var parameters = Parameters;
            if (parameters.GetAttributes().Any())
            {
                var builder = new StringBuilder();
                foreach (var parameter in parameters.GetAttributes())
                {
                    if (string.IsNullOrEmpty(parameter.Key))
                        continue;
                    builder
                        .Append("&")
                        .Append(parameter.Key)
                        .Append("=")
                        .Append(parameter.Value ?? string.Empty);
                }
                if (builder.Length > 0)
                {
                    builder.Replace('&', '?', 0, 1);
                    extraValue = builder.ToString();
                }
            }

            if (string.IsNullOrEmpty(UserName))
            {
                Password = null;
            }

            var uriBuilder = new UriBuilder(Scheme, Host, Port, Path, extraValue)
            {
                UserName = UserName,
                Password = Password
            };
            return uriBuilder.Uri.ToString();
        }

        #endregion Overrides of Object

        #region Public Static Method

        /// <summary>
        /// 根据一个字符串创建一个Url。
        /// </summary>
        /// <param name="url">url字符串。</param>
        /// <returns>Url实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="url"/> 为null。</exception>
        /// <exception cref="UriFormatException"><paramref name="url"/> 格式不正确。</exception>
        public static Url Create(string url)
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            var userInfo = uri.UserInfo;
            string userName = string.Empty, userPassword = string.Empty;
            if (!string.IsNullOrEmpty(userInfo))
            {
                if (userInfo.Contains(":"))
                {
                    var temp = userInfo.Split(new[] { ':' }, 2);
                    userName = temp[0];
                    userPassword = temp[1];
                }
                else
                {
                    userName = userInfo;
                }
            }

            var parameters = new Dictionary<string, string>();

            var query = uri.Query;

            if (!string.IsNullOrEmpty(query))
            {
                query = query.TrimStart('?');
                var temp = query.Split('&');
                foreach (var s in temp)
                {
                    //a=1,b=1,c=1
                    if (s.Contains("="))
                    {
                        var item = s.Split(new[] { '=' }, 2);
                        parameters[item[0]] = item[1];
                    }
                    else //a,b,c
                    {
                        parameters[s] = string.Empty;
                    }
                }
            }

            return new Url(uri.Scheme, userName, userPassword, uri.Host, uri.Port, uri.AbsolutePath, parameters);
        }

        #endregion Public Static Method
    }
}