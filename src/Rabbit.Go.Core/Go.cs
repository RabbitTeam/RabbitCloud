using Castle.DynamicProxy;
using Rabbit.Go.Core;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go
{
    public abstract class Go
    {
        public abstract object CreateInstance(Type type);
    }

    public abstract class GoBase : Go
    {
        private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        #region Overrides of Go

        public override object CreateInstance(Type type)
        {
            return _proxyGenerator.CreateInterfaceProxyWithoutTarget(type, Enumerable.Empty<Type>().ToArray(), new InterceptorAsync(async invocation =>
            {
                var invoker = CreateInvoker(type, invocation.Method);
                var result = await invoker.InvokeAsync(invocation.Arguments);
                return result;
            }));
        }

        #endregion Overrides of Go

        protected abstract IMethodInvoker CreateInvoker(Type type, MethodInfo methodInfo);
    }

    public class DefaultGo : GoBase
    {
        private readonly IMethodDescriptorCollectionProvider _methodDescriptorCollectionProvider;
        private readonly IMethodInvokerFactory _methodInvokerFactory;

        public DefaultGo(IMethodDescriptorCollectionProvider methodDescriptorCollectionProvider, IMethodInvokerFactory methodInvokerFactory)
        {
            _methodDescriptorCollectionProvider = methodDescriptorCollectionProvider;
            _methodInvokerFactory = methodInvokerFactory;
        }

        #region Overrides of Go

        protected override IMethodInvoker CreateInvoker(Type type, MethodInfo methodInfo)
        {
            var descriptors = _methodDescriptorCollectionProvider.Items;
            var descriptor = descriptors.SingleOrDefault(i => i.ClienType == type && i.MethodInfo == methodInfo);
            var invoker = _methodInvokerFactory.CreateInvoker(descriptor);
            return invoker;
        }

        #endregion Overrides of Go
    }
}