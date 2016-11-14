namespace RabbitCloud.Rpc.Abstractions.Utils
{
    public static class MessageIdGenerator
    {
        private static long _id;

        public static long GeneratorId()
        {
            return _id += 1;
        }
    }
}