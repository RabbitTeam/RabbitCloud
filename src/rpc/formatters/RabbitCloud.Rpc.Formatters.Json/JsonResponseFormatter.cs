using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Formatter;
using RabbitCloud.Rpc.Formatters.Json.Utilities;
using System;
using System.IO;
using System.Text;

namespace RabbitCloud.Rpc.Formatters.Json
{
    public class JsonResponseFormatter : IResponseFormatter
    {
        #region Implementation of IResponseFormatter

        public IInputFormatter<IResponse> InputFormatter { get; } = new JsonResponseInputFormatter();
        public IOutputFormatter<IResponse> OutputFormatter { get; } = new JsonResponseOutputFormatter();

        #endregion Implementation of IResponseFormatter

        private class JsonResponseInputFormatter : ResponseInputFormatter
        {
            #region Implementation of IInputFormatter<out IResponse>

            protected override void DoFormat(byte[] data, Response response)
            {
                var json = Encoding.UTF8.GetString(data);

                var obj = JObject.Parse(json);
                var exceptionMessage = obj.SelectToken("Exception")?.Value<string>();
                response.Exception = exceptionMessage == null ? null : new RpcException(exceptionMessage);
                response.Value = StrongType.GetStrongType(obj.SelectToken("Value"));
            }

            #endregion Implementation of IInputFormatter<out IResponse>
        }

        private class JsonResponseOutputFormatter : ResponseOutputFormatter
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

            protected override void DoFormat(IResponse response, MemoryStream memoryStream)
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
                    Value = StrongType.CreateStrongType(response.Value),
                    Exception = GetExceptionMessage(response.Exception)
                };
                var json = JsonConvert.SerializeObject(model, _jsonSerializerSettings);
                var data = Encoding.UTF8.GetBytes(json);
                memoryStream.Write(data, 0, data.Length);
            }

            #endregion Implementation of IOutputFormatter<in IResponse>
        }
    }
}