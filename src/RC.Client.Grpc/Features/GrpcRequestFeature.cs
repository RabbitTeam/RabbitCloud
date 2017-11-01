using Grpc.Core;
using Rabbit.Cloud.Client.Features;
using System;
using System.Threading;

namespace Rabbit.Cloud.Client.Grpc.Features
{
    public interface IGrpcRequestFeature : IRequestFeature
    {
        object Request { get; set; }
        string Host { get; set; }
        Metadata Headers { get; set; }
        DateTime? Deadline { get; set; }
        CallOptions CallOptions { get; set; }
        CancellationToken CancellationToken { get; set; }
    }

    public class GrpcRequestFeature : IGrpcRequestFeature
    {
        #region Implementation of IGrpcRequestFeature

        public object Request { get; set; }
        public string Host { get; set; }
        public Metadata Headers { get; set; }
        public DateTime? Deadline { get; set; }
        public CallOptions CallOptions { get; set; }
        public CancellationToken CancellationToken { get; set; }

        #endregion Implementation of IGrpcRequestFeature

        #region Implementation of IRequestFeature

        public ServiceUrl ServiceUrl { get; set; }

        #endregion Implementation of IRequestFeature
    }
}