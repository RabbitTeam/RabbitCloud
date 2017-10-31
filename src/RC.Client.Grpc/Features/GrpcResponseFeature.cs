namespace Rabbit.Cloud.Client.Grpc.Features
{
    public interface IGrpcResponseFeature
    {
        object Response { get; set; }
    }

    public class GrpcResponseFeature : IGrpcResponseFeature
    {
        #region Implementation of IGrpcResponseFeature

        public object Response { get; set; }

        #endregion Implementation of IGrpcResponseFeature
    }
}