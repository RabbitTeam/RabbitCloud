using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.ApplicationModels;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Grpc
{
    public class GrpcOptions
    {
        public ICollection<TypeInfo> ScanTypes { get; } = new List<TypeInfo>();
        public IList<IApplicationModelConvention> Conventions { get; } = new List<IApplicationModelConvention>();
        public ICollection<ISerializer> Serializers { get; } = new List<ISerializer>();
    }
}