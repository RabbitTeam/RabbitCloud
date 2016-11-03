using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using System;
using System.IO;
using System.Reflection;

namespace RabbitCloud.Rpc.Http.Service
{
    public class HttpCodec : ICodec
    {
        #region Implementation of ICodec

        public void Encode(TextWriter writer, object message)
        {
            var invocation = message as IInvocation;
            var result = message as IResult;
            if (invocation != null)
            {
                writer.Write(JsonConvert.SerializeObject(new
                {
                    invocation.MethodName,
                    invocation.Attributes.Metadata,
                    invocation.Arguments,
                    invocation.ParameterTypes
                }));
            }
            else if (result != null)
            {
                writer.Write(JsonConvert.SerializeObject(new
                {
                    Exception = result.Exception?.Message,
                    result.Value
                }));
            }
            writer.Flush();
        }

        public object Decode(TextReader reader, Type type)
        {
            var isInvocation = typeof(IInvocation).IsAssignableFrom(type);
            var isResult = !isInvocation && typeof(IResult).IsAssignableFrom(type);

            var content = reader.ReadToEnd();
            var obj = JObject.Parse(content);
            if (isInvocation)
            {
                var invocation = obj.ToObject<RpcInvocation>();
                for (var i = 0; i < invocation.Arguments.Length; i++)
                {
                    var argument = (JObject)invocation.Arguments[i];
                    var parameterType = invocation.ParameterTypes[i];
                    invocation.Arguments[i] = argument.ToObject(parameterType);
                }
                return invocation;
            }
            if (isResult)
            {
                var result = obj.ToObject<RpcResult>();
                return result;
            }

            return null;
        }

        #endregion Implementation of ICodec
    }
}