using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Go.ApplicationModels
{
    public class ApplicationModelProviderContext
    {
        public ApplicationModelProviderContext(IEnumerable<TypeInfo> serviceTypes)
        {
            ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
        }

        public IEnumerable<TypeInfo> ServiceTypes { get; }

        public ApplicationModel Result { get; } = new ApplicationModel();
    }
}