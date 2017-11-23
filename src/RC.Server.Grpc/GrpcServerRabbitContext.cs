using Grpc.Core;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Server.Grpc
{
    public class GrpcServerRabbitRequest : IRabbitRequest
    {
        public GrpcServerRabbitContext Context { get; }

        public GrpcServerRabbitRequest(GrpcServerRabbitContext context)
        {
            Context = context;
        }

        #region Implementation of IRabbitRequest

        public ServiceUrl Url { get; set; }

        #endregion Implementation of IRabbitRequest

        public ServerMethodModel ServerMethod { get; set; }
        public object Request { get; set; }
        public ServerCallContext ServerCallContext { get; set; }
    }

    public class GrpcServerRabbitResponse
    {
        public GrpcServerRabbitContext Context { get; }

        public GrpcServerRabbitResponse(GrpcServerRabbitContext context)
        {
            Context = context;
        }

        public Type ResponseType { get; set; }
        public object Response { get; set; }
    }

    public class GrpcServerRabbitContext : IRabbitContext
    {
        public GrpcServerRabbitContext()
        {
            Request = new GrpcServerRabbitRequest(this);
            Response = new GrpcServerRabbitResponse(this);
        }

        #region Implementation of IRabbitContext

        public IFeatureCollection Features { get; } = new FeatureCollection();
        public IServiceProvider RequestServices { get; set; }
        IRabbitRequest IRabbitContext.Request => Request;

        #endregion Implementation of IRabbitContext

        public Func<Task<object>> LogicInvoker { get; set; }
        public GrpcServerRabbitRequest Request { get; }
        public GrpcServerRabbitResponse Response { get; }
    }
}