namespace RabbitCloud.Rpc.Default.Utils
{
    /*    public static class MessageIdGenerator
        {
            private static long _number;

            public static Id GeneratorId()
            {
                var id = Interlocked.Increment(ref _number);
                if (id == long.MaxValue)
                    Interlocked.Exchange(ref _number, 0);
                return id;
            }
        }*/

    public static class MessageIdGenerator
    {
        private static long _id;

        public static long GeneratorId()
        {
            return _id += 1;
        }
    }
}