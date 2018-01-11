using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using System;
using System.Diagnostics;
using System.Linq;

namespace Rabbit.Cloud.Client.Go.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [DebuggerDisplay("TypeFilter: Type={ImplementationType} Order={Order}")]
    public class TypeFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        private ObjectFactory _factory;

        public TypeFilterAttribute(Type type)
        {
            ImplementationType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public object[] Arguments { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of filter to create.
        /// </summary>
        public Type ImplementationType { get; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public bool IsReusable { get; set; }

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (_factory != null) return (IFilterMetadata)_factory(serviceProvider, Arguments);
            var argumentTypes = Arguments?.Select(a => a.GetType())?.ToArray();

            _factory = ActivatorUtilities.CreateFactory(ImplementationType, argumentTypes ?? Type.EmptyTypes);

            return (IFilterMetadata)_factory(serviceProvider, Arguments);
        }
    }
}