using Castle.DynamicProxy;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;
using System;

namespace Rabbit.Cloud.Facade.Internal
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        public static RabbitRequestDelegate RabbitRequestDelegate { get; set; }
        private readonly FacadeOptions _facadeOptions;

        public ProxyFactory(IOptions<FacadeOptions> facadeOptions)
        {
            _facadeOptions = facadeOptions.Value;
        }

        #region Implementation of IProxyFactory

        public object GetProxy(Type type, RabbitRequestDelegate rabbitRequestDelegate)
        {
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, new[] { type }, new ServiceRequestInterceptor(rabbitRequestDelegate, _facadeOptions));
        }

        #endregion Implementation of IProxyFactory
    }
}