using Newtonsoft.Json;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Default.Service.Message;
using System;
using System.IO;
using System.Linq;

namespace RabbitCloud.Rpc.Default.Service
{
    public class Codec : ICodec
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();

        #region Implementation of ICodec

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
                    JsonSerializer.Serialize(writer, new
                    {
                        Id = id,
                        Arguments = requestMessage.Arguments == null || !requestMessage.Arguments.Any() ? null : requestMessage.Arguments,
                        requestMessage.MethodName
                    });
                }
                else if (responseMessage != null)
                {
                    JsonSerializer.Serialize(writer, new
                    {
                        Id = id,
                        responseMessage.ExceptionMessage,
                        responseMessage.Result
                    });
                }
            }
            else
            {
                JsonSerializer.Serialize(writer, message);
            }

            writer.Flush();
        }

        public object Decode(TextReader reader, Type type)
        {
            return JsonSerializer.Deserialize(reader, type);
        }

        #endregion Implementation of ICodec
    }
}