/*using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities
{
    public static class MethodUtilities
    {
        #region Create Method

        public static IMethod CreateMethod(string serviceName, string methodName, MethodType methodType, Type requesType, Type responseType, object requestMarshaller, object responseMarshaller)
        {
            var constructor = Cache.GetConstructor(requesType, responseType);

            var methodTypeExpression = Cache.GetConstant(methodType);
            var serviceNameExpression = Expression.Constant(serviceName);
            var methodNameExpression = Expression.Constant(methodName);
            var requestMarshallerExpression = Expression.Constant(requestMarshaller);
            var responseMarshallerExpression = Expression.Constant(responseMarshaller);

            var newExpression = Expression.New(constructor, methodTypeExpression, serviceNameExpression, methodNameExpression, requestMarshallerExpression, responseMarshallerExpression);

            var factory = Expression.Lambda(newExpression).Compile();

            return (IMethod)factory.DynamicInvoke();
        }

        #endregion Create Method

        #region Help Type

        private static class Cache
        {
            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            private static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                {
                    return (T)cache;
                }
                return (T)(Caches[key] = factory());
            }

            public static ConstructorInfo GetConstructor(Type requesType, Type responseType)
            {
                var key = ("constructor", requesType, responseType);

                return GetCache(key, () => typeof(Method<,>).MakeGenericType(requesType, responseType).GetConstructors().First());
            }

            public static ConstantExpression GetConstant(object value)
            {
                var key = ("constant", value);
                return GetCache(key, () => Expression.Constant(value));
            }
        }

        #endregion Help Type
    }
}*/