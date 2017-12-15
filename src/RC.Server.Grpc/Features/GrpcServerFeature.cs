using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Grpc.Features
{
    public interface IGrpcServerFeature
    {
        ServerCallContext ServerCallContext { get; set; }

        Type ResponseType { get; set; }
        Func<Task<object>> ResponseInvoker { get; set; }
    }

    public class GrpcServerFeature : IGrpcServerFeature
    {
        #region Implementation of IGrpcServerFeature

        public ServerCallContext ServerCallContext { get; set; }
        public Type ResponseType { get; set; }
        public Func<Task<object>> ResponseInvoker { get; set; }

        #endregion Implementation of IGrpcServerFeature
    }
}