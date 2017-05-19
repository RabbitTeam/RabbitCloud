namespace RabbitCloud.Abstractions.Utilities
{
    public class MathUtil
    {
        public static int GetPositive(int originValue)
        {
            return 0x7fffffff & originValue;
        }
    }
}