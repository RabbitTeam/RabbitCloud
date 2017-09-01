using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Discovery.Abstractions
{
    public class ServiceInstance : IServiceInstance
    {
        public ServiceInstance(string serviceId, string host, int port, bool isSecure, Uri uri, IDictionary<string, string> metadata = null)
        {
            ServiceId = serviceId;
            Host = host;
            Port = port;
            IsSecure = isSecure;
            Uri = uri;
            Metadata = metadata ?? new Dictionary<string, string>();
        }

        #region Implementation of IServiceInstance

        public string ServiceId { get; }
        public string Host { get; }
        public int Port { get; }
        public bool IsSecure { get; }
        public Uri Uri { get; }
        public IDictionary<string, string> Metadata { get; }

        #endregion Implementation of IServiceInstance

        public static Uri GetUri(IServiceInstance instance)
        {
            var scheme = instance.IsSecure ? "https" : "http";
            return new Uri($"{scheme}://{instance.Host}:{instance.Port}");
        }

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"ServiceId={ServiceId},Host={Host},Port={Port},IsSecure={IsSecure},Uri={Uri},Metadata={Metadata}";
        }


        #region Equality members

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (!(obj is ServiceInstance instance))
                return false;

            return Port == instance.Port &&
                   IsSecure == instance.IsSecure &&
                   string.Equals(ServiceId, instance.ServiceId) &&
                   string.Equals(Host, instance.Host) &&
                   Metadata == instance.Metadata;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ServiceId != null ? ServiceId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Host != null ? Host.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Port;
                hashCode = (hashCode * 397) ^ IsSecure.GetHashCode();
                hashCode = (hashCode * 397) ^ (Metadata != null ? Metadata.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion Equality members

        #endregion Overrides of Object
    }
}