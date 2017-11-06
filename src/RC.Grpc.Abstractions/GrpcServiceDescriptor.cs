using Grpc.Core;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using System;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public struct GrpcServiceDescriptor
    {
        public string ServiceId { get; private set; }
        public string ServiceName { get; private set; }
        public string MethodName { get; private set; }
        public MethodType MethodType { get; private set; }
        public Type RequesType { get; private set; }
        public Type ResponseType { get; private set; }
        public object RequestMarshaller { get; set; }
        public object ResponseMarshaller { get; set; }

        public IMethod CreateMethod()
        {
            if (string.IsNullOrEmpty(ServiceName))
                throw new ArgumentNullException(nameof(ServiceName));
            if (string.IsNullOrEmpty(MethodName))
                throw new ArgumentNullException(nameof(MethodName));
            if (RequesType == null)
                throw new ArgumentNullException(nameof(RequesType));
            if (ResponseType == null)
                throw new ArgumentNullException(nameof(ResponseType));
            if (RequestMarshaller == null)
                throw new ArgumentNullException(nameof(RequestMarshaller));
            if (ResponseMarshaller == null)
                throw new ArgumentNullException(nameof(ResponseMarshaller));

            return MethodUtilities.CreateMethod(ServiceName, MethodName, MethodType, RequesType, ResponseType,
                RequestMarshaller, ResponseMarshaller);
        }

        public static GrpcServiceDescriptor Create(Type serviceType, MethodInfo methodInfo, Func<Type, object> marshallerFactory)
        {
            var requestType = methodInfo.GetRequestType();
            var responseType = methodInfo.GetResponseType();

            return Create(serviceType, methodInfo, requestType, responseType, marshallerFactory(requestType), marshallerFactory(responseType));
        }

        public static GrpcServiceDescriptor Create(Type serviceType, MethodInfo methodInfo, object requestMarshaller = null, object responseMarshaller = null)
        {
            var requestType = methodInfo.GetRequestType();
            var responseType = methodInfo.GetResponseType();

            return Create(serviceType, methodInfo, requestType, responseType, requestMarshaller, responseMarshaller);
        }

        public static GrpcServiceDescriptor Create(Type serviceType, MethodInfo methodInfo, Type requestType, Type responseType, object requestMarshaller = null, object responseMarshaller = null)
        {
            (string serviceName, string methodName) = ReflectionUtilities.GetServiceNames(serviceType, methodInfo);

            return new GrpcServiceDescriptor
            {
                ServiceId = $"/{serviceName}/{methodName}",
                ServiceName = serviceName,
                MethodName = methodName,
                MethodType = methodInfo.GetMethodType(),
                RequesType = requestType,
                ResponseType = responseType,
                RequestMarshaller = requestMarshaller,
                ResponseMarshaller = responseMarshaller
            };
        }
    }
}