using Microsoft.Extensions.DependencyInjection;
using Rabbit.Go.Abstractions.Filters;
using System;
using System.Diagnostics;

namespace Rabbit.Go.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [DebuggerDisplay("ServiceFilter: Type={ServiceType} Order={Order}")]
    public class ServiceFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        /// <summary>
        /// Instantiates a new <see cref="ServiceFilterAttribute"/> instance.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of filter to find.</param>
        public ServiceFilterAttribute(Type type)
        {
            ServiceType = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of filter to find.
        /// </summary>
        public Type ServiceType { get; }

        /// <inheritdoc />
        public bool IsReusable { get; set; }

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var service = serviceProvider.GetRequiredService(ServiceType);

            if (!(service is IFilterMetadata filter))
            {
                throw new InvalidOperationException($"FormatFilterFactoryAttribute_TypeMustImplementIFilter {typeof(ServiceFilterAttribute).Name} {typeof(IFilterMetadata).Name}");
            }

            return filter;
        }
    }
}