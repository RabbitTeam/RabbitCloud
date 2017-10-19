using Castle.DynamicProxy;
using Rabbit.Cloud.Client.Abstractions;
using System;

namespace Rabbit.Cloud.Guise.Internal
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