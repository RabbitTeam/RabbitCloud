namespace Rabbit.Go
{
    public static class GoExtensions
    {
        public static T CreateInstance<T>(this Go go)
        {
            return (T)go.CreateInstance(typeof(T));
        }
    }
}