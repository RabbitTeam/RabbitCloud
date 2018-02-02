using System;
using System.Collections.Generic;

namespace Rabbit.Go.Core.GoModels
{
    public class GoModelProviderContext
    {
        public GoModelProviderContext(IList<Type> types)
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        public IList<Type> Types { get; }

        public GoModel Result { get; } = new GoModel();
    }
}