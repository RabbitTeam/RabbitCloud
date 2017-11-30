using ProtoBuf;
using System;
using System.IO;
using System.Reflection;

namespace Rabbit.Cloud.Serialization.Protobuf
{
    public class ProtobufSerializer : Serializer
    {
        #region Overrides of Serializer

        protected override void DoSerialize(Stream stream, object instance)
        {
            if (!IsProtobuf(instance.GetType()))
                return;

            ProtoBuf.Serializer.Serialize(stream, instance);
        }

        protected override object DoDeserialize(Type type, Stream stream)
        {
            return IsProtobuf(type) ? ProtoBuf.Serializer.Deserialize(type, stream) : null;
        }

        #endregion Overrides of Serializer

        private static bool IsProtobuf(MemberInfo type)
        {
            return type.GetCustomAttribute<ProtoContractAttribute>() != null;
        }
    }
}