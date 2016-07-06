using ProtoBuf;
using Rabbit.Rpc.Codec.ProtoBuffer.Utilitys;
using System;

namespace Rabbit.Rpc.Codec.ProtoBuffer.Messages
{
    [ProtoContract]
    public class ProtoBufferDynamicItem
    {
        #region Constructor

        public ProtoBufferDynamicItem()
        {
        }

        public ProtoBufferDynamicItem(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            TypeName = value.GetType().AssemblyQualifiedName;
            Content = SerializerUtilitys.Serialize(value);
        }

        #endregion Constructor

        #region Property

        [ProtoMember(1)]
        public string TypeName { get; set; }

        [ProtoMember(2)]
        public byte[] Content { get; set; }

        #endregion Property

        #region Public Method

        public object Get()
        {
            if (Content == null || TypeName == null)
                return null;

            return SerializerUtilitys.Deserialize(Content, Type.GetType(TypeName));
        }

        #endregion Public Method
    }
}