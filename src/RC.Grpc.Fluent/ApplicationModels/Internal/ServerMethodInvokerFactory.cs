using System;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels.Internal
{
    public class ServerMethodInvokerFactory : IServerMethodInvokerFactory
    {
        private readonly IServiceProvider _services;

        public ServerMethodInvokerFactory(IServiceProvider services)
        {
            _services = services;
        }

        #region Implementation of IServerMethodInvokerFactory

        public ServerMethodInvoker CreateInvoker(ServerMethodModel serverMethod)
        {
            return new DefaultServerMethodInvoker(serverMethod, _services);
        }

        #endregion Implementation of IServerMethodInvokerFactory
    }
}