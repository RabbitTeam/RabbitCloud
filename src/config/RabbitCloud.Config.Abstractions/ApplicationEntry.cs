using System;
using System.Linq;

namespace RabbitCloud.Config.Abstractions
{
    public class ApplicationEntry
    {
        public string Name { get; set; }
        public ReferenceEntry[] References { get; set; }
        public ServiceEntry[] Services { get; set; }

        public object GetReference(string referenceId)
        {
            var reference = References.SingleOrDefault(i => i.Config.Id.Equals(referenceId, StringComparison.OrdinalIgnoreCase));
            return reference?.ServiceProxy;
        }

        public T GetReference<T>(string referenceId)
        {
            return (T)GetReference(referenceId);
        }

        public T GetReference<T>()
        {
            return GetReference<T>(typeof(T).Name);
        }
    }
}