using System;

namespace RabbitCloud.Abstractions
{
    public struct ServiceKey
    {
        public ServiceKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Group = "default";
            Version = "1.0.0";
        }

        public string Name { get; set; }
        public string Group { get; set; }

        public string Version { get; set; }

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>

        public override string ToString()
        {
            return $"{Group ?? "default"}/{Name}/{Version ?? "1.0.0"}";
        }

        #endregion Overrides of Object

        #region Equality members

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            var key = (ServiceKey)obj;
            return key.Name == Name && key.Group == Group && key.Version == Version;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Group != null ? Group.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion Equality members
    }
}