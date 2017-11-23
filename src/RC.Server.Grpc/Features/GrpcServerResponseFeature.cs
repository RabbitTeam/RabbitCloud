using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Grpc.Features
{
    public interface IGrpcServerResponseFeature
    {
        Type ResponseType { get; set; }
        object Response { get; set; }
        Func<Task<object>> ResponseInvoker { get; set; }
    }

    public class GrpcServerResponseFeature : IGrpcServerResponseFeature
    {
        #region Implementation of IGrpcServerResponseFeature

        public Type ResponseType { get; set; }
        public object Response { get; set; }
        public Func<Task<object>> ResponseInvoker { get; set; }

        #endregion Implementation of IGrpcServerResponseFeature
    }

    public static class GrpcServerResponseFeatureExtensions
    {
        public static async Task<object> GetResponseAsync(this IGrpcServerResponseFeature feature)
        {
            return feature.Response ?? (feature.Response = await feature.ResponseInvoker());
        }

        public static async Task<TResponse> GetResponseAsync<TResponse>(this IGrpcServerResponseFeature feature)
        {
            var response = await feature.GetResponseAsync();
            return (TResponse)response;
        }
    }
}