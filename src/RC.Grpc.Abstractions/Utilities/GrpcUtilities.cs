using Grpc.Core;
using Rabbit.Cloud.Grpc.Abstractions.ApplicationModels;
using System;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities
{
    public static class MarshallerUtilities2
    {
        private static Delegate GetCreateMarshallerDelegate(Type type)
        {
            var serializerFuncType = Expression.GetFuncType(type, typeof(byte[]));
            var serializerFuncParameterExpression = Expression.Parameter(serializerFuncType);
            var deserializerFuncType = Expression.GetFuncType(typeof(byte[]), type);
            var deserializerFuncParameterExpression = Expression.Parameter(deserializerFuncType);

            var createCallExpression = Expression.Call(typeof(Marshallers), nameof(Marshallers.Create), new[] { type },
                serializerFuncParameterExpression, deserializerFuncParameterExpression);
            return Expression.Lambda(createCallExpression, serializerFuncParameterExpression,
                deserializerFuncParameterExpression).Compile();
        }

        private static Delegate GetSerializerDelegate(Type type)
        {
            var codecParameterExpression = Expression.Parameter(typeof(ICodec));
            var typeParameterExpression = Expression.Parameter(type);
            var objectRequestParameterExpression = Expression.Convert(typeParameterExpression, typeof(object));
            var encodeCallExpression = Expression.Call(codecParameterExpression, nameof(ICodec.Encode), null,
                objectRequestParameterExpression);

            return Expression.Lambda(encodeCallExpression, codecParameterExpression, typeParameterExpression).Compile();
        }

        private static Delegate GetDeserializerDelegate(Type type)
        {
//            var typeParameterExpression = Expression.Parameter(typeof(Type));
            var typeExpression = Expression.Constant(type);
            var codecParameterExpression = Expression.Parameter(typeof(ICodec));
            var dataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
            var decodeCallExpression = Expression.Convert(Expression.Call(codecParameterExpression, nameof(ICodec.Decode), null, dataParameterExpression, typeExpression), type);
            var decodeLambdaExpression = Expression.Lambda(decodeCallExpression, dataParameterExpression);

            return decodeLambdaExpression.Compile();
        }

        public static object CreateMarshaller(Type type, ICodec codec)
        {
            var d = GetSerializerDelegate(type);
            var t = GetDeserializerDelegate(type);
            return null;
            var codecConstantExpression = Expression.Constant(codec);
            var typeConstantExpression = Expression.Constant(type);

            var typeParameterExpression = Expression.Parameter(type);
            var objectRequestParameterExpression = Expression.Convert(typeParameterExpression, typeof(object));
            var encodeCallExpression = Expression.Call(codecConstantExpression, nameof(ICodec.Encode), null, objectRequestParameterExpression);

            var encodeLambdaExpression = Expression.Lambda(encodeCallExpression, typeParameterExpression);

            var dataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
            var decodeCallExpression = Expression.Convert(Expression.Call(codecConstantExpression, nameof(ICodec.Decode), null, dataParameterExpression, typeConstantExpression), type);

            var decodeLambdaExpression = Expression.Lambda(decodeCallExpression, dataParameterExpression);

            var en = encodeLambdaExpression.Compile();
            var de = decodeLambdaExpression.Compile();

            var dd = GetCreateMarshallerDelegate(type);
            dd.DynamicInvoke(en, de);

            return null;

            //            Serializer
            //            Deserializer
        }
    }
}