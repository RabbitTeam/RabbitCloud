using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.Proxy
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IInterceptor[] _interceptors;
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public ProxyFactory(IEnumerable<IInterceptor> interceptors)
        {
            if (interceptors == null)
                throw new ArgumentNullException(nameof(interceptors));
            _interceptors = interceptors.ToArray();
        }

        #region Implementation of IProxyFactory

        public object CreateInterfaceProxy(Type interfaceType)
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, new Type[0], _interceptors);
        }

        public object CreateInterfaceProxy(Type interfaceType, object interfaceTarget)
        {
            return ProxyGenerator.CreateInterfaceProxyWithTargetInterface(interfaceType, interfaceTarget, _interceptors);
        }

        #endregion Implementation of IProxyFactory
    }
}