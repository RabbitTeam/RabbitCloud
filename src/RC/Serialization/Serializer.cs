using Rabbit.Cloud.Abstractions.Serialization;
using System;
using System.IO;

namespace Rabbit.Cloud.Serialization
{
    public abstract class Serializer : ISerializer
    {
        #region Implementation of ISerializer

        bool ISerializer.CanHandle(Type type)
        {
            return CanHandle(type);
        }

        public void Serialize(Stream stream, object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            DoSerialize(stream, instance);
        }

        public object Deserialize(Type type, Stream stream)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return DoDeserialize(type, stream);
        }

        protected abstract bool CanHandle(Type type);

        protected abstract void DoSerialize(Stream stream, object instance);

        protected abstract object DoDeserialize(Type type, Stream stream);

        #endregion Implementation of ISerializer
    }
}