using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Features;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client
{
    public interface ICodec
    {
        object Encode(object body);

        object Decode(object data);
    }

    public interface ISerializer
    {
        void Serialize(object instance, Stream stream);

        object Deserialize(Stream stream, Type type);
    }

    internal class SerializerCodec : ICodec
    {
        private readonly ISerializer _serializer;
        private readonly ICodecFeature _codecFeature;

        public SerializerCodec(ISerializer serializer, ICodecFeature codecFeature)
        {
            _serializer = serializer;
            _codecFeature = codecFeature;
        }

        #region Implementation of ICodec

        public object Encode(object body)
        {
            var memoryStream = new MemoryStream();
            _serializer.Serialize(body, memoryStream);
            return memoryStream;
        }

        public object Decode(object data)
        {
            return _serializer.Deserialize(data as Stream, _codecFeature.ResponseType);
        }

        #endregion Implementation of ICodec
    }

    internal class InternalJsonSerializer : ISerializer
    {
        private readonly JsonSerializer _jsonSerializer;

        public InternalJsonSerializer()
        {
            _jsonSerializer = new JsonSerializer();
        }

        #region Implementation of ISerializer

        public void Serialize(object instance, Stream stream)
        {
            var writer = new StreamWriter(stream);
            _jsonSerializer.Serialize(writer, instance);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }

        public object Deserialize(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream))
                return _jsonSerializer.Deserialize(reader, type);
        }

        #endregion Implementation of ISerializer
    }

    public class CodecMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ILogger<CodecMiddleware> _logger;

        public CodecMiddleware(RabbitRequestDelegate next, ILogger<CodecMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var codecFeature = context.Features.Get<ICodecFeature>();
            if (codecFeature == null)
                context.Features.Set(codecFeature = new CodecFeature());

            if (codecFeature.Codec == null)
            {
                codecFeature.Codec = new SerializerCodec(new InternalJsonSerializer(), codecFeature);
            }

            await _next(context);
        }
    }
}