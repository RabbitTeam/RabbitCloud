using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Formatters.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitCloud.Rpc.Formatters.Json
{
    public class JsonRequestFormatter : IRequestFormatter
    {
        #region Implementation of IRequestFormatter

        public IInputFormatter<IRequest> InputFormatter { get; } = new JsonRequestInputFormatter();
        public IOutputFormatter<IRequest> OutputFormatter { get; } = new JsonRequestOutputFormatter();

        #endregion Implementation of IRequestFormatter

        private class JsonRequestInputFormatter : IInputFormatter<IRequest>
        {
            #region Implementation of IInputFormatter<out IRequest>

            public IRequest Format(byte[] data)
            {
                var json = Encoding.UTF8.GetString(data);
                var obj = JObject.Parse(json);

                var attachments = obj.SelectToken("Attachments")?.ToObject<Dictionary<string, string>>();

                var arguments = obj.SelectToken("Arguments")?.Select(StrongType.GetStrongType).ToArray();

                var requestId = obj.SelectToken("RequestId")?.Value<long>();
                var methodDescriptor = obj.SelectToken("MethodDescriptor")?.ToObject<MethodDescriptor>();
                if (requestId == null)
                    throw new ArgumentException($"missing {nameof(requestId)}.");
                if (methodDescriptor == null)
                    throw new ArgumentException($"missing {nameof(methodDescriptor)}.");

                var request = new Request(attachments)
                {
                    MethodDescriptor = methodDescriptor.Value,
                    Arguments = arguments,
                    RequestId = requestId.Value
                };
                return request;
            }

            #endregion Implementation of IInputFormatter<out IRequest>
        }

        private class JsonRequestOutputFormatter : IOutputFormatter<IRequest>
        {
            private readonly JsonSerializerSettings _jsonSerializerSettings;

            public JsonRequestOutputFormatter()
            {
                _jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
            }

            #region Implementation of IOutputFormatter<in IRequest>

            public byte[] Format(IRequest instance)
            {
                var model = new
                {
                    instance.MethodDescriptor,
                    instance.RequestId,
                    instance.Attachments,
                    Arguments = instance.Arguments?.Select(StrongType.CreateStrongType)
                };
                var json = JsonConvert.SerializeObject(model, _jsonSerializerSettings);
                return Encoding.UTF8.GetBytes(json);
            }

            #endregion Implementation of IOutputFormatter<in IRequest>
        }
    }
}