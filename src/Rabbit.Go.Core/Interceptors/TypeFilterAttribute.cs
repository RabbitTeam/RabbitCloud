using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Rabbit.Go.Interceptors
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class TypeFilterAttribute : Attribute, IInterceptorFactory, IOrderedInterceptor
    {
        private ObjectFactory _factory;
        public object[] Arguments { get; set; }
        public Type ImplementationType { get; }

        public TypeFilterAttribute(Type type)
        {
            ImplementationType = type;
        }

        #region Implementation of IInterceptorFactory

        public bool IsReusable { get; set; }

        public IInterceptorMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (_factory != null) return (IInterceptorMetadata)_factory(serviceProvider, Arguments);
            var argumentTypes = Arguments?.Select(a => a.GetType())?.ToArray();

            _factory = ActivatorUtilities.CreateFactory(ImplementationType, argumentTypes ?? Type.EmptyTypes);

            return (IInterceptorMetadata)_factory(serviceProvider, Arguments);
        }

        #endregion Implementation of IInterceptorFactory

        #region Implementation of IOrderedInterceptor

        public int Order { get; set; }

        #endregion Implementation of IOrderedInterceptor
    }
}