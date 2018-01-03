using Grpc.Core;

namespace Rabbit.Cloud.Client.Grpc.Features
{
    public interface IGrpcFeature
    {
        Channel Channel { get; set; }
        CallOptions? CallOptions { get; set; }
        object RequestMarshaller { get; set; }
        object ResponseMarshaller { get; set; }
    }

    public class GrpcFeature : IGrpcFeature
    {
        #region Implementation of IGrpcFeature

        public Channel Channel { get; set; }
        public CallOptions? CallOptions { get; set; }
        public object RequestMarshaller { get; set; }
        public object ResponseMarshaller { get; set; }

        #endregion Implementation of IGrpcFeature
    }
}