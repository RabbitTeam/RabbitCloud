using MessagePack;
using System;
using System.IO;
using System.Reflection;
using PackSerializer = MessagePack.MessagePackSerializer.NonGeneric;

namespace Rabbit.Cloud.Serialization.MessagePack
{
    public class MessagePackSerializer : Serializer
    {
        #region Overrides of Serializer

        protected override bool CanHandle(Type type)
        {
            return IsMessagePack(type);
        }

        protected override void DoSerialize(Stream stream, object instance)
        {
            PackSerializer.Serialize(instance.GetType(), stream, instance);
        }

        protected override object DoDeserialize(Type type, Stream stream)
        {
            return PackSerializer.Deserialize(type, stream);
        }

        #endregion Overrides of Serializer

        private static bool IsMessagePack(MemberInfo type)
        {
            return type.GetCustomAttribute<MessagePackObjectAttribute>() != null;
        }
    }
}