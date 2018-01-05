using Castle.DynamicProxy;
using Rabbit.Cloud.Client.Go.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.Go
{
    public class ProxyFactory : IProxyFactory
    {
        private readonly IInterceptor[] _interceptors;
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public ProxyFactory(IEnumerable<IInterceptor> interceptors)
        {
            _interceptors = interceptors.ToArray();
        }

        #region Implementation of IProxyFactory

        public object CreateProxy(Type interfaceType)
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(interfaceType, new Type[0], _interceptors);
        }

        #endregion Implementation of IProxyFactory
    }
}