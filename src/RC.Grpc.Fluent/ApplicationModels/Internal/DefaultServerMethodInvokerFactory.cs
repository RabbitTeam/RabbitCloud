using Microsoft.Extensions.Logging;
using System;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels.Internal
{
    public class DefaultServerMethodInvokerFactory : IServerMethodInvokerFactory
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DefaultServerMethodInvoker> _logger;

        public DefaultServerMethodInvokerFactory(IServiceProvider services, ILogger<DefaultServerMethodInvoker> logger)
        {
            _services = services;
            _logger = logger;
        }

        #region Implementation of IServerMethodInvokerFactory

        public IServerMethodInvoker CreateInvoker(ServerMethodModel serverMethod)
        {
            return new DefaultServerMethodInvoker(serverMethod, _services, _logger);
        }

        #endregion Implementation of IServerMethodInvokerFactory
    }
}