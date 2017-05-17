using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Formatters.Json.Utilities;
using System;
using System.Text;

namespace RabbitCloud.Rpc.Formatters.Json
{
    public class JsonResponseFormatter : IResponseFormatter
    {
        #region Implementation of IResponseFormatter

        public IInputFormatter<IResponse> InputFormatter { get; } = new JsonResponseInputFormatter();
        public IOutputFormatter<IResponse> OutputFormatter { get; } = new JsonResponseOutputFormatter();

        #endregion Implementation of IResponseFormatter

        private class JsonResponseInputFormatter : IInputFormatter<IResponse>
        {
            #region Implementation of IInputFormatter<out IResponse>

            public IResponse Format(byte[] data)
            {
                var json = Encoding.UTF8.GetString(data);

                var obj = JObject.Parse(json);
                var exceptionMessage = obj.SelectToken("Exception")?.Value<string>();
                var response = new Response
                {
                    RequestId = obj.Value<long>("RequestId"),
                    Exception = exceptionMessage == null ? null : new RpcException(exceptionMessage),
                    Value = StrongType.GetStrongType(obj.SelectToken("Value"))
                };

                return response;
            }

            #endregion Implementation of IInputFormatter<out IResponse>
        }

        private class JsonResponseOutputFormatter : IOutputFormatter<IResponse>
        {
            private readonly JsonSerializerSettings _jsonSerializerSettings;

            public JsonResponseOutputFormatter()
            {
                _jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
            }

            public JsonResponseOutputFormatter(JsonSerializerSettings jsonSerializerSettings)
            {
                _jsonSerializerSettings = jsonSerializerSettings;
            }

            #region Implementation of IOutputFormatter<in IResponse>

            public byte[] Format(IResponse instance)
            {
                string GetExceptionMessage(Exception exception)
                {
                    if (exception == null)
                        return null;
                    var builder = new StringBuilder();
                    builder
                        .AppendLine("Message:")
                        .AppendLine(exception.Message)
                        .AppendLine("Source:")
                        .AppendLine(exception.Source)
                        .AppendLine("StackTrace:")
                        .AppendLine(exception.StackTrace);

                    return builder.ToString();
                }
                var model = new
                {
                    instance.RequestId,
                    Value = StrongType.CreateStrongType(instance.Value),
                    Exception = GetExceptionMessage(instance.Exception)
                };
                var json = JsonConvert.SerializeObject(model, _jsonSerializerSettings);
                return Encoding.UTF8.GetBytes(json);
            }

            #endregion Implementation of IOutputFormatter<in IResponse>
        }
    }
}