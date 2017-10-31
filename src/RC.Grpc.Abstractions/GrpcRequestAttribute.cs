using System;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class GrpcRequestAttribute : Attribute
    {
    }
}