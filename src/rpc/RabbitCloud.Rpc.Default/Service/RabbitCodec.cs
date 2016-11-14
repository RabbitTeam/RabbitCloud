using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Codec;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using RabbitCloud.Rpc.Abstractions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitCloud.Rpc.Default.Service
{
    public class RabbitCodec : ICodec
    {
        #region Implementation of ICodec

        /// <summary>
        /// 对内容进行编码。
        /// </summary>
        /// <param name="content">内容。</param>
        /// <returns>编码后的结果。</returns>
        public object Encode(object content)
        {
            var request = content as IRequest;
            if (request != null)
            {
                var json = JsonConvert.SerializeObject(new
                {
                    Arguments = request.Arguments?.Select(GetTypedValue),
                    request.InterfaceName,
                    request.MethodName,
                    request.ParamtersType,
                    request.RequestId,
                    Parameters = request.GetParameters()
                });
                return Encoding.UTF8.GetBytes(json);
            }
            var response = content as IResponse;
            if (response != null)
            {
                var json = JsonConvert.SerializeObject(new
                {
                    Exception = response.Exception?.Message,
                    response.RequestId,
                    Result = GetTypedValue(response.Result),
                    Parameters = response.GetParameters()
                });
                return Encoding.UTF8.GetBytes(json);
            }
            throw new NotSupportedException(content.GetType().FullName);
        }

        /// <summary>
        /// 对内容进行解码。
        /// </summary>
        /// <param name="content">内容。</param>
        /// <param name="type">内容类型。</param>
        /// <returns>解码后的结果。</returns>
        public object Decode(object content, Type type)
        {
            JObject obj = null;

            if (content is IEnumerable<byte>)
                content = Encoding.UTF8.GetString((byte[])content);
            if (content is string)
                obj = JObject.Parse(content.ToString());

            if (obj == null)
                throw new NotSupportedException(content.GetType().FullName);

            if (type == typeof(IRequest))
            {
                return new DefaultRequest
                {
                    Arguments = ((JArray)obj["Arguments"]).Select(GetTypedValue).ToArray(),
                    InterfaceName = obj["InterfaceName"].Value<string>(),
                    MethodName = obj["MethodName"].Value<string>(),
                    ParamtersType = obj["ParamtersType"].ToObject<string[]>(),
                    RequestId = obj["RequestId"].Value<long>()
                };
            }
            if (type == typeof(IResponse))
            {
                var exceptionMessage = obj.Value<string>("Exception");
                return new DefaultResponse
                {
                    Exception = exceptionMessage == null ? null : new RpcException(exceptionMessage),
                    RequestId = obj["RequestId"].ToObject<long>(),
                    Result = GetTypedValue(obj["Result"])
                };
            }
            throw new NotSupportedException(type.FullName);
        }

        #endregion Implementation of ICodec

        #region Private Method

        private static object GetTypedValue(object value)
        {
            return value == null ? null : GetTypedValue(value.GetType().AssemblyQualifiedName, value);
        }

        private static object GetTypedValue(string type, object value)
        {
            return new { Type = type, Value = value };
        }

        private static object GetTypedValue(JToken token)
        {
            if (token == null)
                return null;

            var typeToken = token.SelectToken("Type");
            var valueToken = token.SelectToken("Value");

            if (valueToken == null)
                return null;

            var typeString = typeToken.Value<string>();
            var type = Type.GetType(typeString);
            return valueToken.ToObject(type);
        }

        #endregion Private Method
    }
}