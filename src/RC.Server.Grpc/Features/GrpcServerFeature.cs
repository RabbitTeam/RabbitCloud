using Rabbit.Cloud.ApplicationModels;

namespace Rabbit.Cloud.Server.Grpc.Features
{
    public interface IGrpcServerFeature
    {
        MethodModel ServerMethod { get; set; }
    }

    public class GrpcServerFeature : IGrpcServerFeature
    {
        #region Implementation of IGrpcServerFeature

        public MethodModel ServerMethod { get; set; }

        #endregion Implementation of IGrpcServerFeature
    }
}