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
    }
}