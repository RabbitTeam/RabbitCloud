using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Core.Interceptors;
using System;
using System.Collections.Generic;

namespace Rabbit.Go.Core
{
    public class GoOptions
    {
        public GoOptions()
        {
            Interceptors = new InterceptorCollection();
            Conventions = new List<IGoModelConvention>();
            Types = new List<Type>();
        }

        public IList<IGoModelConvention> Conventions { get; }
        public InterceptorCollection Interceptors { get; }
        public IList<Type> Types { get; }
    }
}