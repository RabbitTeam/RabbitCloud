using Newtonsoft.Json;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RC.Facade.Formatters.Json
{
    public class JsonInputFormatter : IInputFormatter
    {
        protected JsonSerializerSettings SerializerSettings { get; }
        private readonly Internal.JsonArrayPool<char> _charPool;
        private JsonSerializer _serializer;

        public JsonInputFormatter(JsonSerializerSettings jsonSerializerSettings, ArrayPool<char> charPool)
        {
            SerializerSettings = jsonSerializerSettings;
            _charPool = new Internal.JsonArrayPool<char>(charPool);
        }

        #region Implementation of IInputFormatter

        public bool CanWriteResult(InputFormatterCanWriteContext context)
        {
            return context.ContentType.Equals("application/json");
        }

        public async Task WriteAsync(InputFormatterWriteContext context)
        {
            var request = context.RabbitContext.Request.RequestMessage;

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    WriteObject(streamWriter, context.Object);
                    await streamWriter.FlushAsync();

                    var content = new ByteArrayContent(memoryStream.ToArray());
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    request.Content = content;
                }
            }
        }

        private void WriteObject(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            using (var jsonWriter = CreateJsonWriter(writer))
            {
                var jsonSerializer = CreateJsonSerializer();
                jsonSerializer.Serialize(jsonWriter, value);
            }
        }

        #endregion Implementation of IInputFormatter

        private JsonWriter CreateJsonWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var jsonWriter = new JsonTextWriter(writer)
            {
                ArrayPool = _charPool,
                CloseOutput = false,
                AutoCompleteOnClose = false
            };

            return jsonWriter;
        }

        private JsonSerializer CreateJsonSerializer()
        {
            return _serializer ?? (_serializer = JsonSerializer.Create(SerializerSettings));
        }
    }
}