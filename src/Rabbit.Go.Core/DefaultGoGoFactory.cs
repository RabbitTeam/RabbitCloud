using Castle.DynamicProxy;
using Rabbit.Go.Core;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go
{
    public abstract class GoFactoryBase : IGoFactory
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        protected abstract MethodDescriptor GetMethodDescriptor(Type type, MethodInfo methodInfo);

        protected abstract IMethodInvoker CreateInvoker(Type type, MethodInfo methodInfo);

        #region Implementation of IGoFactory

        public object CreateInstance(Type type)
        {
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, Enumerable.Empty<Type>().ToArray(), new InterceptorAsync(async invocation =>
            {
                var invoker = CreateInvoker(type, invocation.Method);
                var result = await invoker.InvokeAsync(invocation.Arguments);
                return result;
            }));
        }

        #endregion Implementation of IGoFactory
    }

    public class DefaultGoGoFactory : GoFactoryBase
    {
        private readonly IMethodDescriptorCollectionProvider _methodDescriptorCollectionProvider;
        private readonly IMethodInvokerFactory _methodInvokerFactory;

        public DefaultGoGoFactory(IMethodDescriptorCollectionProvider methodDescriptorCollectionProvider, IMethodInvokerFactory methodInvokerFactory)
        {
            _methodDescriptorCollectionProvider = methodDescriptorCollectionProvider;
            _methodInvokerFactory = methodInvokerFactory;
        }

        #region Overrides of Go

        protected override MethodDescriptor GetMethodDescriptor(Type type, MethodInfo methodInfo)
        {
            var descriptors = _methodDescriptorCollectionProvider.Items;
            var descriptor = descriptors.SingleOrDefault(i => i.ClienType == type && i.MethodInfo == methodInfo);
            return descriptor;
        }

        protected override IMethodInvoker CreateInvoker(Type type, MethodInfo methodInfo)
        {
            {
                var descriptor = GetMethodDescriptor(type, methodInfo);
                var invoker = _methodInvokerFactory.CreateInvoker(descriptor);
                return invoker;
            }
        }

        #endregion Overrides of Go
    }
}