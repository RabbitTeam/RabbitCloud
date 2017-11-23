using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;

namespace Rabbit.Cloud.Server.Grpc.Features
{
    public interface IGrpcServerFeature
    {
        ServerMethodModel ServerMethod { get; set; }
    }

    public class GrpcServerFeature : IGrpcServerFeature
    {
        #region Implementation of IGrpcServerFeature

        public ServerMethodModel ServerMethod { get; set; }

        #endregion Implementation of IGrpcServerFeature
    }
}