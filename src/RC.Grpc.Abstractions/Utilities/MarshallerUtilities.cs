using Grpc.Core;
using System;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities
{
    public static class MarshallerUtilities
    {
        private static readonly ParameterExpression DataParameterExpression;

        static MarshallerUtilities()
        {
            DataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
        }

        public static object CreateMarshaller(Type type, Expression serializerDelegateExpression, Expression deserializerDelegateExpression)
        {
            var createMethodExpression = Expression.Call(typeof(Marshallers), nameof(Marshallers.Create), new[] { type }, serializerDelegateExpression, deserializerDelegateExpression);

            var factory = Expression.Lambda(createMethodExpression).Compile();
            return factory.DynamicInvoke();
        }

        public static object CreateMarshaller(Type type, Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            var requestParameterExpression = Expression.Parameter(type);
            var objectRequestParameterExpression = Expression.Convert(requestParameterExpression, typeof(object));
            var serializerCallExpression = Expression.Call(Expression.Constant(serializer.Target), serializer.Method, objectRequestParameterExpression);

            var serializerDelegate = Expression.Lambda(serializerCallExpression, requestParameterExpression).Compile();
            var serializerDelegateExpression = Expression.Constant(serializerDelegate);

            var dataParameterExpression = DataParameterExpression;
            var deserializerCallExpression = Expression.Convert(Expression.Call(Expression.Constant(deserializer.Target), deserializer.Method, dataParameterExpression), type);

            var deserializerDelegate = Expression.Lambda(deserializerCallExpression, dataParameterExpression).Compile();
            var deserializerDelegateExpression = Expression.Constant(deserializerDelegate);

            return CreateMarshaller(type, serializerDelegateExpression, deserializerDelegateExpression);
        }
    }
}