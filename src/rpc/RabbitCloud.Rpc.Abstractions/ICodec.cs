using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的编解码器。
    /// </summary>
    public interface ICodec
    {
        /// <summary>
        /// 对消息进行编码。
        /// </summary>
        /// <param name="message">消息实例。</param>
        /// <returns>编码后的内容。</returns>
        object Encode(object message);

        /// <summary>
        /// 对消息进行解码。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <param name="type">内容类型。</param>
        /// <returns>解码后的内容。</returns>
        object Decode(object message, Type type);
    }

    public class JsonCodec : ICodec
    {
        #region Implementation of ICodec

        /// <summary>
        /// 对消息进行编码。
        /// </summary>
        /// <param name="message">消息实例。</param>
        /// <returns>编码后的内容。</returns>
        public object Encode(object message)
        {
            throw new NotImplementedException();
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

            if (type == typeof(Invocation))
            {
                var array = (JArray)obj.SelectToken("Body.Arguments");
                var arguments = array.Cast<JObject>().Select(i =>
                  {
                      var argumentTypeString = i.Value<string>("Type");
                      var argumentType = Type.GetType(argumentTypeString);
                      if (argumentType == null)
                          throw new Exception($"解析参数类型时发生了错误，找不到类型: {argumentTypeString}");
                      var argument = i.Property("Content").ToObject(argumentType);
                      return argument;
                  }).ToArray();
                var invocation = new Invocation
                {
                    Arguments = arguments
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
    }
}