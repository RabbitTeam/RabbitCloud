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

        protected override bool DoSerialize(Stream stream, object instance)
        {
            if (!IsMessagePack(instance.GetType()))
                return false;

            PackSerializer.Serialize(instance.GetType(), stream, instance);

            return true;
        }

        protected override object DoDeserialize(Type type, Stream stream)
        {
            return IsMessagePack(type) ? PackSerializer.Deserialize(type, stream) : null;
        }

        #endregion Overrides of Serializer

        private static bool IsMessagePack(MemberInfo type)
        {
            return type.GetCustomAttribute<MessagePackObjectAttribute>() != null;
        }
    }
}