using Grpc.Core;

namespace Rabbit.Cloud.Client.Grpc.Features
{
    public interface IGrpcFeature
    {
        CallInvoker CallInvoker { get; set; }
        IMethod Method { get; set; }
        CallOptions? CallOptions { get; set; }
    }

    public class GrpcFeature : IGrpcFeature
    {
        #region Implementation of IGrpcFeature

        public CallInvoker CallInvoker { get; set; }
        public IMethod Method { get; set; }
        public CallOptions? CallOptions { get; set; }

        #endregion Implementation of IGrpcFeature
    }
}