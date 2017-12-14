using ProtoBuf;
using System;
using System.IO;
using System.Reflection;

namespace Rabbit.Cloud.Serialization.Protobuf
{
    public class ProtobufSerializer : Serializer
    {
        #region Overrides of Serializer

        protected override bool CanHandle(Type type)
        {
            return IsProtobuf(type);
        }

        protected override void DoSerialize(Stream stream, object instance)
        {
            ProtoBuf.Serializer.Serialize(stream, instance);
        }

        protected override object DoDeserialize(Type type, Stream stream)
        {
            return ProtoBuf.Serializer.Deserialize(type, stream);
        }

        #endregion Overrides of Serializer

        private static bool IsProtobuf(MemberInfo type)
        {
            return type.GetCustomAttribute<ProtoContractAttribute>() != null;
        }
    }
}