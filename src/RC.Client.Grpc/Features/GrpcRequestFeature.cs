using Grpc.Core;
using Rabbit.Cloud.Client.Features;

namespace Rabbit.Cloud.Client.Grpc.Features
{
    public interface IGrpcRequestFeature : IRequestFeature
    {
        object Request { get; set; }
        string Host { get; set; }
        CallOptions CallOptions { get; set; }
    }

    public class GrpcRequestFeature : IGrpcRequestFeature
    {
        #region Implementation of IGrpcRequestFeature

        public object Request { get; set; }
        public string Host { get; set; }
        public CallOptions CallOptions { get; set; }

        #endregion Implementation of IGrpcRequestFeature

        #region Implementation of IRequestFeature

        public ServiceUrl ServiceUrl { get; set; }

        #endregion Implementation of IRequestFeature
    }
}