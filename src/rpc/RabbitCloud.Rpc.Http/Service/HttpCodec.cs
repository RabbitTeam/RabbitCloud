using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Serialization;
using System;
using System.IO;
using System.Reflection;

namespace RabbitCloud.Rpc.Http.Service
{
    public class HttpCodec : ICodec
    {
        private readonly ISerializer _serializer;

        public HttpCodec(ISerializer serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of ICodec

        public void Encode(TextWriter writer, object message)
        {
            var invocation = message as IInvocation;
            var result = message as IResult;
            if (invocation != null)
            {
                _serializer.Serialize(writer, new
                {
                    invocation.MethodName,
                    invocation.Metadata,
                    invocation.Arguments,
                    invocation.ParameterTypes
                });
            }
            else if (result != null)
            {
                _serializer.Serialize(writer, new
                {
                    Exception = result.Exception?.Message,
                    result.Value
                });
            }
        }

        public object Decode(TextReader reader, Type type)
        {
            var isInvocation = typeof(IInvocation).IsAssignableFrom(type);
            var isResult = typeof(IResult).IsAssignableFrom(type);

            if (isInvocation)
            {
                var invocation = (IInvocation)_serializer.Deserialize(reader, typeof(Invocation));
                for (var i = 0; i < invocation.Arguments.Length; i++)
                {
                    var argument = invocation.Arguments[i];
                    var parameterType = invocation.ParameterTypes[i];
                    invocation.Arguments[i] = _serializer.DeserializeByString(argument.ToString(), parameterType);
                }
                return invocation;
            }
            if (isResult)
            {
                var result = (IResult)_serializer.Deserialize(reader, typeof(Result));
                return result;
            }

            return null;
        }

        #endregion Implementation of ICodec
    }
}