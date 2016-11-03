using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Abstractions.Feature;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Service.Message;
using System;
using System.Collections.Generic;
using System.IO;

namespace RabbitCloud.Rpc.Default.Service
{
    public class RabbitCodec : ICodec
    {
        #region Implementation of ICodec

        /// <summary>
        /// 编码。
        /// </summary>
        /// <param name="writer">写入器。</param>
        /// <param name="message">消息。</param>
        public void Encode(TextWriter writer, object message)
        {
            var rpcMessage = message as RpcMessage;
            if (rpcMessage != null)
            {
                var requestMessage = message as RequestMessage;
                var responseMessage = message as ResponseMessage;
                object id;
                if (rpcMessage.Id.IsInteger)
                    id = (long)rpcMessage.Id;
                else
                    id = rpcMessage.Id.ToString();
                if (requestMessage != null)
                {
                    var invocation = requestMessage.Invocation;
                    var arguments = invocation.Arguments;
                    var content = JsonConvert.SerializeObject(new
                    {
                        Id = id,
                        Invocation = new
                        {
                            invocation.MethodName,
                            Arguments = arguments,
                            invocation.ParameterTypes,
                            invocation.Attributes.Metadata
                        }
                    });
                    writer.Write(content);
                }
                else if (responseMessage != null)
                {
                    var content = JsonConvert.SerializeObject(new
                    {
                        Id = id,
                        responseMessage.Result
                    });
                    writer.Write(content);
                }
            }

            writer.Flush();
        }

        /// <summary>
        /// 解码。
        /// </summary>
        /// <param name="reader">读取器。</param>
        /// <param name="type">消息类型。</param>
        /// <returns>消息实例。</returns>
        public object Decode(TextReader reader, Type type)
        {
            var content = reader.ReadToEnd();

            if (type == typeof(RequestMessage))
            {
                var obj = JObject.Parse(content);
                var id = obj.Property("Id").Value;
                var message = new RequestMessage
                {
                    Id = id.Type == JTokenType.Integer ? (Id)id.Value<long>() : (Id)id.Value<string>(),
                    Invocation = new RpcInvocation
                    {
                        Arguments = obj.SelectToken("Invocation.Arguments").ToObject<object[]>(),
                        Attributes = new DefaultMetadataFeature(obj.SelectToken("Invocation.Metadata").ToObject<IDictionary<string, object>>()),
                        MethodName = obj.SelectToken("Invocation.MethodName").Value<string>(),
                        ParameterTypes = obj.SelectToken("Invocation.ParameterTypes").ToObject<Type[]>()
                    }
                };
                var invocation = message.Invocation;
                for (var i = 0; i < invocation.Arguments.Length; i++)
                {
                    var argument = (JObject)invocation.Arguments[i];
                    var parameterType = invocation.ParameterTypes[i];
                    invocation.Arguments[i] = argument.ToObject(parameterType);
                }
                return message;
            }
            if (type == typeof(ResponseMessage))
            {
                return JsonConvert.DeserializeObject<ResponseMessage>(content);
            }

            throw new NotSupportedException($"不支持的类型: {type}");
        }

        #endregion Implementation of ICodec
    }
}