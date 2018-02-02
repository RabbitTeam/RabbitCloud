using System;

namespace Rabbit.Go.Core
{
    public interface IGoFactory
    {
        object CreateInstance(Type type);
    }
}