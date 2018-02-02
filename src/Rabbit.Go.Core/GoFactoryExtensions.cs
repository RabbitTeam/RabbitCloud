using Rabbit.Go.Core;

namespace Rabbit.Go
{
    public static class GoFactoryExtensions
    {
        public static T CreateInstance<T>(this IGoFactory goFactory)
        {
            return (T)goFactory.CreateInstance(typeof(T));
        }
    }
}