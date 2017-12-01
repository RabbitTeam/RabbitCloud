using Rabbit.Cloud.Abstractions;
using System;

namespace Rabbit.Cloud.Grpc.Fluent.Utilities
{
    public class RpcExceptionUtilities
    {
        public static RabbitRpcException NotFoundSerializer(Type type)
        {
            return new RabbitRpcException(RabbitRpcExceptionCode.Business, $"Can not find a suitable serializer to serialize,type:{type}");
        }
    }
}