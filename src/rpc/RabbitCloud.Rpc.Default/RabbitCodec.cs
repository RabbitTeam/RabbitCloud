using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RabbitCloud.Rpc.Default
{
    public class RabbitCodec : ICodec
    {
        #region Implementation of ICodec

        /// <summary>
        /// 对消息进行编码。
        /// </summary>
        /// <param name="message">消息实例。</param>
        /// <returns>编码后的内容。</returns>
        public object Encode(object message)
        {
            if (message is RabbitInvocation)
            {
                var invocation = (RabbitInvocation)message;
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                {
                    invocation.Headers,
                    invocation.Path,
                    invocation.QueryString,
                    invocation.Scheme,
                    Arguments = invocation.Arguments.Select(GetTypeParameter)
                }));
            }
            return null;
        }

        /// <summary>
        /// 对消息进行解码。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <param name="type">内容类型。</param>
        /// <returns>解码后的内容。</returns>
        public object Decode(object message, Type type)
        {
            if (type.IsInstanceOfType(message))
                return message;

            JObject obj;
            if (message is JObject)
                obj = (JObject)message;
            else if (message is byte[])
                obj = GetObjByBytes((byte[])message);
            else if (message is string)
                obj = GetObjByString((string)message);
            else
                throw new NotSupportedException($"not support type: {message.GetType().FullName}");

            if (type == typeof(RabbitInvocation))
            {
                var arguments = ((JArray)obj.SelectToken("Arguments")).Select(GetArgument).ToArray();
                var invocation = new RabbitInvocation
                {
                    Arguments = arguments,
                    Path = obj.Value<string>("Path"),
                    QueryString = obj.Value<string>("QueryString"),
                    Scheme = obj.Value<string>("Scheme"),
                    Headers = obj["Headers"].ToObject<IDictionary<string, string>>()
                };
                return invocation;
            }

            return null;
        }

        #endregion Implementation of ICodec

        private static JObject GetObjByBytes(byte[] buffer)
        {
            return GetObjByString(Encoding.UTF8.GetString(buffer));
        }

        private static JObject GetObjByString(string message)
        {
            return JObject.Parse(message);
        }

        private static object GetArgument(JToken token)
        {
            var typeString = token.Value<string>("Type");
            var type = Type.GetType(typeString);
            if (type == null)
                throw new Exception($"解析参数类型时发生了错误，找不到类型: {typeString}");
            return token["Content"].ToObject(type);
        }

        private static object GetTypeParameter(object obj)
        {
            return new { Type = obj.GetType().AssemblyQualifiedName, Content = obj };
        }
    }
}