using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rabbit.Cloud.Grpc.Utilities
{
    public static class MarshallerUtilities
    {
        public static object CreateGenericMarshaller(Type type, Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            //todo: Consider whether you need to cache
            var serializerConstantExpression = Expression.Constant(serializer);
            var parameterExpression = Expression.Parameter(typeof(object));
            var serializerDelegate = Expression.Lambda(Expression.Invoke(serializerConstantExpression, parameterExpression), parameterExpression).Compile();

            var deserializerConstantExpression = Expression.Constant(deserializer);
            var dataParameterExpression = Expression.Parameter(typeof(byte[]), "data");
            var deserializerDelegate = Expression.Lambda(Expression.Convert(Expression.Invoke(deserializerConstantExpression, dataParameterExpression), type), dataParameterExpression).Compile();

            var createMarshallerFactory = GetCreateMarshallerFactory(type);
            return createMarshallerFactory(serializerDelegate, deserializerDelegate);
        }

        #region Private Method

        private static Func<object, object, object> GetCreateMarshallerFactory(Type type)
        {
            return Cache.GetCache(("CreateMarshaller", type), () =>
            {
                var serializerFuncType = Expression.GetFuncType(type, typeof(byte[]));
                var serializerFuncParameterExpression = Expression.Parameter(typeof(object));

                var deserializerFuncType = Expression.GetFuncType(typeof(byte[]), type);
                var deserializerFuncParameterExpression = Expression.Parameter(typeof(object));

                var createCallExpression = Expression.Call(typeof(Marshallers), nameof(Marshallers.Create), new[] { type }, Expression.Convert(serializerFuncParameterExpression, serializerFuncType), Expression.Convert(deserializerFuncParameterExpression, deserializerFuncType));
                return Expression.Lambda<Func<object, object, object>>(createCallExpression, serializerFuncParameterExpression, deserializerFuncParameterExpression).Compile();
            });
        }

        #endregion Private Method

        #region Help Type

        internal class Cache
        {
            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            public static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                    return (T)cache;
                return (T)(Caches[key] = factory());
            }
        }

        #endregion Help Type
    }
}