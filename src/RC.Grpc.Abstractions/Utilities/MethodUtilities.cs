using Google.Protobuf;
using Grpc.Core;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Utilities
{
    public static class MarshallerUtilities
    {
        private static MethodInfo MarshallerCreateMethodInfo;

        static MarshallerUtilities()
        {
            MarshallerCreateMethodInfo = typeof(Marshallers).GetMethod("Create");
        }

        public static object CreateMarshaller(Type type, Func<object, byte[]> serializer, Func<byte[], object> deserializer)
        {
            var serializerType = Expression.GetFuncType(type, typeof(byte[]));
            var deserializerType = Expression.GetFuncType(typeof(byte[]), type);

            var serializerExpression = Expression.Constant(serializer);
            var deserializerExpression = Expression.Constant(deserializer);


//            Expression.Invoke(serializerType,)
//            Expression.Lambda(serializerType,)

            //            var createMethod = typeof(Marshallers).GetMethod("Create").MakeGenericMethod(type);

            //            var serializerMethod = typeof(MarshallerUtilities).GetMethod(nameof(Serializer));
            //            var deserializerMethod = typeof(MarshallerUtilities).GetMethod(nameof(Deserializer));

            //            serializerMethod = serializerMethod.MakeGenericMethod(type);
            //            deserializerMethod = deserializerMethod.MakeGenericMethod(type);

            //            var serializerDelegate = serializerMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(type, typeof(byte[])));
            //            var deserializerDelegate = deserializerMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(byte[]), type));

            var marshaller = createMethod.Invoke(null, new object[] { serializer, deserializer });
            return marshaller;
        }

        private static byte[] Serializer<T>(T request)
        {
            if (request is IMessage message)
            {
                return message.ToByteArray();
            }
            return null;
        }

        private static T Deserializer<T>(byte[] bytes)
        {
            var messageParserType = typeof(MessageParser<>).MakeGenericType(typeof(T));

            var newExpression = Expression.New(typeof(T));
            var delegateType = typeof(Func<>).MakeGenericType(typeof(T));

            var createInstanceExpression = Expression.Lambda(delegateType, newExpression);

            var createInstanceFactory = (Func<T>)createInstanceExpression.Compile();

            var ms = (MessageParser)Activator.CreateInstance(messageParserType, createInstanceFactory);

            return (T)ms.ParseFrom(bytes);
        }
    }

    public static class MethodUtilities
    {
        public static IMethod CreateMethod(MethodInfo method, Type requesType, Type responseType, object requestMarshaller, object responseMarshaller)
        {
            var type = method.DeclaringType;
            var serviceName = $"{type.Namespace.ToLower()}.{type.Name}";
            var methodName = method.Name;

            /*var responseMarshaller = _marshallerFactory.GetMarshaller(responseType);
            var requesTypeMarshaller = _marshallerFactory.GetMarshaller(requesType);*/

            var methodType = typeof(Method<,>).MakeGenericType(requesType, responseType);
            var methodInstance = (IMethod)Activator.CreateInstance(methodType, MethodType.Unary, serviceName, methodName, requestMarshaller, responseMarshaller);

            return methodInstance;
        }
    }
}