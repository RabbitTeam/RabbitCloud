using Rabbit.Cloud.Client.Abstractions.Codec;
using Rabbit.Cloud.Client.Serialization;
using System;
using System.IO;

namespace Rabbit.Cloud.Client.Codec
{
    public class SerializerCodec : ICodec
    {
        private readonly ISerializer _serializer;
        private readonly Type _requestType;
        private readonly Type _responseType;

        public SerializerCodec(ISerializer serializer, Type requestType, Type responseType)
        {
            _serializer = serializer;
            _requestType = requestType;
            _responseType = responseType;
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
            return _serializer.Deserialize(data as Stream, _responseType);
        }

        #endregion Implementation of ICodec
    }
}