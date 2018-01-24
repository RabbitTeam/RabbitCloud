using System;

namespace Rabbit.Go
{
    public interface IGo
    {
        object CreateInstance(Type type);
    }

    public static class GoExtensions
    {
        public static T CreateInstance<T>(this IGo go)
        {
            return (T)go.CreateInstance(typeof(T));
        }
    }
}