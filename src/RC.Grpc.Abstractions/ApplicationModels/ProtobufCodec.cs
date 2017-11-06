using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Abstractions.ApplicationModels
{
    public class ProtobufCodec : Codec
    {
        protected override byte[] DoEncode(object model)
        {
            if (model is IMessage message)
                return message.ToByteArray();

            throw new ArgumentException($"{nameof(model)} not {nameof(IMessage)} type.");
        }

        protected override object DoDecode(byte[] data, Type type)
        {
            if (!typeof(IMessage).IsAssignableFrom(type))
                throw new ArgumentException($"{type} not {nameof(IMessage)} type.");

            var factory = Cache.GetNewDelegate(type);
            var instance = factory.DynamicInvoke();

            var message = (IMessage)instance;
            message.MergeFrom(data);

            return instance;
        }

        #region Help Type

        private static class Cache
        {
            private static readonly IDictionary<Type, Delegate> Delegates = new Dictionary<Type, Delegate>();

            public static Delegate GetNewDelegate(Type type)
            {
                if (Delegates.TryGetValue(type, out var value))
                    return value;
                return Delegates[type] = Expression.Lambda(Expression.New(type)).Compile();
            }
        }

        #endregion Help Type
    }
}