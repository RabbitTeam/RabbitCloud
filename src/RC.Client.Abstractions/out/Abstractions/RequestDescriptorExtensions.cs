using System;

namespace Rabbit.Cloud.Client.Abstractions.Abstractions
{
    public static class RequestDescriptorExtensions
    {
        public static T GetProperty<T>(this RequestDescriptor requestDescriptor)
        {
            if (requestDescriptor == null)
            {
                throw new ArgumentNullException(nameof(requestDescriptor));
            }

            if (requestDescriptor.Properties.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }
            return default(T);
        }

        public static void SetProperty<T>(this RequestDescriptor requestDescriptor, T value)
        {
            if (requestDescriptor == null)
            {
                throw new ArgumentNullException(nameof(requestDescriptor));
            }

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            requestDescriptor.Properties[typeof(T)] = value;
        }
    }
}