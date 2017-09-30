using Castle.DynamicProxy;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;
using System;
using Rabbit.Cloud.Client.Abstractions;

namespace Rabbit.Cloud.Facade.Internal
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IServiceProvider _services;
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public ProxyFactory(IServiceProvider services)
        {
            _services = services;
        }

        #region Implementation of IProxyFactory

        public object GetProxy(Type type, RabbitRequestDelegate rabbitRequestDelegate)
        {
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, new[] { type }, new ServiceRequestInterceptor(rabbitRequestDelegate, _services));
        }

        #endregion Implementation of IProxyFactory
    }
}