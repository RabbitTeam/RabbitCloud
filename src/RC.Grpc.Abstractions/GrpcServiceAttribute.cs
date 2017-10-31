using System;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class GrpcServiceAttribute : Attribute
    {
        public string FullName { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
    }
}