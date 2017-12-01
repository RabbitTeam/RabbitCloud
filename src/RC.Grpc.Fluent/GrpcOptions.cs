using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent
{
    public class GrpcOptions
    {
        public ICollection<TypeInfo> ScanTypes { get; } = new List<TypeInfo>();
        public IList<IApplicationModelConvention> Conventions { get; } = new List<IApplicationModelConvention>();
        public ICollection<ISerializer> Serializers { get; } = new List<ISerializer>();
    }
}