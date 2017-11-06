using System;

namespace Rabbit.Cloud.Grpc.Abstractions.ApplicationModels
{
    public interface ICodec
    {
        byte[] Encode(object model);

        object Decode(byte[] data, Type type);
    }

    public abstract class Codec : ICodec
    {
        public byte[] Encode(object model)
        {
            return model == null ? null : DoEncode(model);
        }

        public object Decode(byte[] data, Type type)
        {
            if (data == null)
                return null;
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsClass || type.IsAbstract)
                throw new ArgumentException($"{nameof(type)} not class.");

            return DoDecode(data, type);
        }

        protected abstract byte[] DoEncode(object model);

        protected abstract object DoDecode(byte[] data, Type type);
    }
}