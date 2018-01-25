using Rabbit.Go.Interceptors;
using System.Collections.Generic;

namespace Rabbit.Go.Core
{
    public class GoOptions
    {
        public GoOptions()
        {
            Interceptors = new List<IInterceptorMetadata>();
        }

        public IList<IInterceptorMetadata> Interceptors { get; }
    }
}