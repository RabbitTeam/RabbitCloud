namespace Rabbit.Go.Filters
{
    public static class FilterScope
    {
        public static readonly int First = 0;
        public static readonly int Global = 10;
        public static readonly int Client = 20;
        public static readonly int Request = 30;
        public static readonly int Last = 100;
    }
}