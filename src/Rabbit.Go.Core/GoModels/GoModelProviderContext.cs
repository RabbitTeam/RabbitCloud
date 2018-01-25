using System;
using System.Collections.Generic;

namespace Rabbit.Go.Core.GoModels
{
    public class GoModelProviderContext
    {
        public GoModelProviderContext(IEnumerable<Type> types)
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        public IEnumerable<Type> Types { get; }

        public GoModel Result { get; } = new GoModel();
    }
}