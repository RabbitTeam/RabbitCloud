using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Formatter;
using RabbitCloud.Rpc.Formatter;
using RabbitCloud.Rpc.Formatters.Json.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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

        private class JsonRequestInputFormatter : RequestInputFormatter
        {
            #region Overrides of RequestInputFormatter

            protected override void DoFormat(byte[] data, Request request)
            {
                var json = Encoding.UTF8.GetString(data);
                var obj = JObject.Parse(json);

                var attachments = obj.SelectToken("Attachments")?.ToObject<Dictionary<string, string>>();

                var arguments = obj.SelectToken("Arguments")?.Select(StrongType.GetStrongType).ToArray();

                var methodDescriptor = obj.SelectToken("MethodDescriptor")?.ToObject<MethodDescriptor>();
                if (methodDescriptor == null)
                    throw new ArgumentException($"missing {nameof(methodDescriptor)}.");

                request.Attachments = attachments;
                request.Arguments = arguments;
                request.MethodDescriptor = methodDescriptor.Value;
            }

            #endregion Overrides of RequestInputFormatter
        }

        private class JsonRequestOutputFormatter : RequestOutputFormatter
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

            protected override void DoFormat(IRequest request, MemoryStream memoryStream)
            {
                var model = new
                {
                    request.MethodDescriptor,
                    request.Attachments,
                    Arguments = request.Arguments?.Select(StrongType.CreateStrongType)
                };
                var json = JsonConvert.SerializeObject(model, _jsonSerializerSettings);
                var data = Encoding.UTF8.GetBytes(json);
                memoryStream.Write(data, 0, data.Length);
            }

            #endregion Implementation of IOutputFormatter<in IRequest>
        }
    }
}