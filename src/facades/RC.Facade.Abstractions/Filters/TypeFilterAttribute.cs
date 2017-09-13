using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class TypeFilterAttribute : Attribute, IFilterFactory
    {
        private ObjectFactory _factory;

        public TypeFilterAttribute(Type type)
        {
            ImplementationType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public object[] Arguments { get; set; }
        public Type ImplementationType { get; }
        public int Order { get; set; }

        #region Implementation of IFilterFactory

        public bool IsReusable { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (_factory == null)
            {
                var argumentTypes = Arguments?.Select(a => a.GetType()).ToArray();

                _factory = ActivatorUtilities.CreateFactory(ImplementationType, argumentTypes ?? Type.EmptyTypes);
            }
            return (IFilterMetadata)_factory(serviceProvider, Arguments);
        }

        #endregion Implementation of IFilterFactory
    }
}