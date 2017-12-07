using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.ApplicationModels
{
    public class ApplicationModelProviderContext
    {
        public ApplicationModelProviderContext(IEnumerable<TypeInfo> types)
        {
            Types = types.ToArray();
        }

        public IReadOnlyCollection<TypeInfo> Types { get; }

        public ApplicationModel Result { get; } = new ApplicationModel();
    }
}