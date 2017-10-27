using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rabbit.Cloud.Grpc.Server
{
    public class DefaultServerServiceDefinitionProviderOptions
    {
        public IEnumerable<Type> Types { get; set; }

        public Func<Type, object> Factory { get; set; }
    }

    public class MarshallerFactory
    {
        private readonly IDictionary<Type, object> _marshallers = new Dictionary<Type, object>();

        public object GetMarshaller(Type type)
        {
            if (_marshallers.TryGetValue(type, out var marshaller))
                return marshaller;

            return _marshallers[type] = CreateMarshaller(type);
        }

        private object CreateMarshaller(Type type)
        {
            var createMethod = typeof(Marshallers).GetMethod("Create").MakeGenericMethod(type);

            var serializerDelegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(type, typeof(byte[])), this, GetType().GetMethod(nameof(Serializer)).MakeGenericMethod(type));
            var deserializerDelegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(byte[]), type), this, GetType().GetMethod(nameof(Deserializer)).MakeGenericMethod(type));

            var marshaller = createMethod.Invoke(null, new object[] { serializerDelegate, deserializerDelegate });
            return marshaller;
        }

        public byte[] Serializer<T>(T request)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
            if (request is IMessage message)
            {
                return message.ToByteArray();
            }
            return null;
        }

        public T Deserializer<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
            var messageParserType = typeof(MessageParser<>).MakeGenericType(typeof(T));

            var newExpression = Expression.New(typeof(T));
            var delegateType = typeof(Func<>).MakeGenericType(typeof(T));

            var createInstanceExpression = Expression.Lambda(delegateType, newExpression);

            var createInstanceFactory = (Func<T>)createInstanceExpression.Compile();

            var ms = (MessageParser)Activator.CreateInstance(messageParserType, createInstanceFactory);

            return (T)ms.ParseFrom(bytes);
        }
    }

    public class MethodFactory
    {
        private readonly MarshallerFactory _marshallerFactory;

        public MethodFactory(MarshallerFactory marshallerFactory)
        {
            _marshallerFactory = marshallerFactory;
        }

        private readonly IDictionary<MethodInfo, IMethod> _methods = new Dictionary<MethodInfo, IMethod>();

        public object GetMethod(MethodInfo methodInfo, Type requesType, Type responseType)
        {
            if (_methods.TryGetValue(methodInfo, out var method))
                return method;
            return _methods[methodInfo] = CreateMethod(methodInfo, requesType, responseType);
        }

        private IMethod CreateMethod(MethodInfo method, Type requesType, Type responseType)
        {
            var type = method.DeclaringType;
            var serviceName = $"{type.Namespace.ToLower()}.{type.Name}";
            var methodName = method.Name;

            var responseMarshaller = _marshallerFactory.GetMarshaller(responseType);
            var requesTypeMarshaller = _marshallerFactory.GetMarshaller(requesType);

            var methodType = typeof(Method<,>).MakeGenericType(requesType, responseType);
            var methodInstance = (IMethod)Activator.CreateInstance(methodType, MethodType.Unary, serviceName, methodName, requesTypeMarshaller, responseMarshaller);

            return methodInstance;
        }
    }

    public class DefaultServerServiceDefinitionProvider : IServerServiceDefinitionProvider
    {
        private readonly DefaultServerServiceDefinitionProviderOptions _options;

        public DefaultServerServiceDefinitionProvider(DefaultServerServiceDefinitionProviderOptions options)
        {
            _options = options;
        }

        private readonly MethodFactory _methodFactory = new MethodFactory(new MarshallerFactory());

        public IEnumerable<ServerServiceDefinition> GetDefinitions()
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            foreach (var type in _options.Types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    var responseType = method.ReturnType;

                    if (typeof(Task).IsAssignableFrom(responseType) && responseType.IsGenericType)
                    {
                        responseType = responseType.GetGenericArguments()[0];
                    }

                    var requesType = method.GetParameters().FirstOrDefault()?.ParameterType;

                    var grpcMethod = _methodFactory.GetMethod(method, requesType, responseType);

                    var delegateType = typeof(UnaryServerMethod<,>).MakeGenericType(requesType, responseType);
                    var requestParameter = Expression.Parameter(requesType, "request");
                    var contextParameter = Expression.Parameter(typeof(ServerCallContext), "context");

                    var factory = _options.Factory;

                    var instancExpression = Expression.Invoke(Expression.Constant(factory), Expression.Constant(type));
                    var serviceInstanceExpression = Expression.Convert(instancExpression, type);

                    //                    var callExpression = Expression.Call(serviceInstanceExpression, type.GetMethod(method.Name), requestParameter, contextParameter);
                    var callExpression = Expression.Call(serviceInstanceExpression, type.GetMethod(method.Name), requestParameter);

                    var func = Expression.Lambda(delegateType, callExpression, requestParameter, contextParameter).Compile();

                    var addMethodInfo = builder.GetType().GetMethods().First(i => i.Name == "AddMethod")
                        .MakeGenericMethod(requesType, responseType);

                    addMethodInfo.Invoke(builder, new[] { grpcMethod, func });
                }
            }

            yield return builder.Build();
        }
    }
}