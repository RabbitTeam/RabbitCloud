using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Serialization;
using RabbitCloud.Rpc.Default.Service.Message;
using System;
using System.IO;

namespace RabbitCloud.Rpc.Default.Service
{
    public class DefaultCodec : ICodec
    {
        private readonly ISerializer _serializer;

        public DefaultCodec(ISerializer serializer)
        {
            _serializer = serializer;
        }

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
                    var invocation = requestMessage.Invocation;
                    var arguments = invocation.Arguments;
                    _serializer.Serialize(writer, new
                    {
                        Id = id,
                        Invocation = new
                        {
                            invocation.MethodName,
                            Arguments = arguments,
                            invocation.ParameterTypes,
                            invocation.Metadata
                        }
                    });
                }
                else if (responseMessage != null)
                {
                    _serializer.Serialize(writer, new
                    {
                        Id = id,
                        responseMessage.Result,
                        responseMessage.Metadata
                    });
                }
            }

            writer.Flush();
        }

        public object Decode(TextReader reader, Type type)
        {
            var obj = _serializer.Deserialize(reader, type);

            var requestMessage = obj as RequestMessage;
            if (requestMessage != null)
            {
                var invocation = requestMessage.Invocation;
                for (var i = 0; i < invocation.Arguments.Length; i++)
                {
                    var argument = invocation.Arguments[i];
                    var parameterType = invocation.ParameterTypes[i];
                    invocation.Arguments[i] = _serializer.DeserializeByString(argument.ToString(), parameterType);
                }
            }

            return obj;
        }

        #endregion Implementation of ICodec
    }
}