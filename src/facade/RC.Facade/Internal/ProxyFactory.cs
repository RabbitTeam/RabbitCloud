using Castle.DynamicProxy;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade.Internal
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();
        private readonly ServiceRequestInterceptor _serviceRequestInterceptor;
        public static RabbitRequestDelegate RabbitRequestDelegate { get; set; }

        public ProxyFactory(IOptions<FacadeOptions> facadeOptions)
        {
            _serviceRequestInterceptor = new ServiceRequestInterceptor(RabbitRequestDelegate, facadeOptions.Value);
        }

        #region Implementation of IProxyFactory

        public T GetProxy<T>()
        {
            var type = typeof(T);
            return (T)_proxyGenerator.CreateInterfaceProxyWithoutTarget(type, new[] { type }, _serviceRequestInterceptor);
        }

        #endregion Implementation of IProxyFactory
    }
}