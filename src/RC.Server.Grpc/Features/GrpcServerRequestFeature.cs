using Grpc.Core;
using Rabbit.Cloud.Application.Features;

namespace Rabbit.Cloud.Server.Grpc.Features
{
    public interface IGrpcServerRequestFeature : IRequestFeature
    {
        object Request { get; set; }
        ServerCallContext ServerCallContext { get; set; }
    }

    public class GrpcServerRequestFeature : IGrpcServerRequestFeature
    {
        #region Implementation of IRequestFeature

        public ServiceUrl ServiceUrl { get; set; }

        #endregion Implementation of IRequestFeature

        #region Implementation of IGrpcServerRequestFeature

        public object Request { get; set; }
        public ServerCallContext ServerCallContext { get; set; }

        #endregion Implementation of IGrpcServerRequestFeature
    }
}