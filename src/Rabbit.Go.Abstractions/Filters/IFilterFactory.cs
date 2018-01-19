using System;

namespace Rabbit.Go.Abstractions.Filters
{
    public interface IFilterFactory : IFilterMetadata
    {
        /// <summary>
        /// Gets a value that indicates if the result of <see cref="CreateInstance(IServiceProvider)"/>
        /// can be reused across requests.
        /// </summary>
        bool IsReusable { get; }

        /// <summary>
        /// Creates an instance of the executable filter.
        /// </summary>
        /// <param name="serviceProvider">The request <see cref="IServiceProvider"/>.</param>
        /// <returns>An instance of the executable filter.</returns>
        IFilterMetadata CreateInstance(IServiceProvider serviceProvider);
    }
}