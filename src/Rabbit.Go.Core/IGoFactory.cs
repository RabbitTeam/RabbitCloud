using System;

namespace Rabbit.Go.Core
{
    public interface IGoFactory
    {
        object CreateInstance(Type type);
    }

    public static class GoFactoryExtensions
    {
        public static T CreateInstance<T>(this IGoFactory goFactory)
        {
            return (T)goFactory.CreateInstance(typeof(T));
        }
    }
}