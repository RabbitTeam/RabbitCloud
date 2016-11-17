using System;
using System.Collections.Generic;

namespace RabbitCloud.Config.Abstractions.Config
{
    public abstract class Config
    {
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string GetAttribute(string name, string def = null)
        {
            string value;
            return Attributes.TryGetValue(name, out value) ? value : def;
        }

        public void SetAttribute(string name, string value)
        {
            Attributes[name] = value;
        }
    }
}