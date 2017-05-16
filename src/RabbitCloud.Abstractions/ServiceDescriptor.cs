namespace RabbitCloud.Abstractions
{
    public class ServiceDescriptor
    {
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