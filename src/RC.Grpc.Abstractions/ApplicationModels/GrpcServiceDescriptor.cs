using System;

namespace Rabbit.Cloud.Grpc.Abstractions.ApplicationModels
{
    public class GrpcServiceDescriptor
    {
        public string ServiceId { get; set; }
        public Type RequesType { get; set; }
        public Type ResponseType { get; set; }
        public ICodec RequestCodec { get; set; }
        public ICodec ResponseCodec { get; set; }
    }
}