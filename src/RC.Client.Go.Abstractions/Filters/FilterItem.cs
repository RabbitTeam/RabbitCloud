using System;

namespace Rabbit.Cloud.Client.Go.Abstractions.Filters
{
    public class FilterItem
    {
        public FilterItem(FilterDescriptor descriptor)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public FilterItem(FilterDescriptor descriptor, IFilterMetadata filter)
            : this(descriptor)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        /// <summary>
        /// Gets the <see cref="FilterDescriptor"/> containing the filter metadata.
        /// </summary>
        public FilterDescriptor Descriptor { get; }

        /// <summary>
        /// Gets or sets the executable <see cref="IFilterMetadata"/> associated with <see cref="Descriptor"/>.
        /// </summary>
        public IFilterMetadata Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not <see cref="Filter"/> can be reused across requests.
        /// </summary>
        public bool IsReusable { get; set; }
    }
}