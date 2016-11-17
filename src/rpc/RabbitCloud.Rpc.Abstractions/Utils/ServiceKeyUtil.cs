namespace RabbitCloud.Rpc.Abstractions.Utils
{
    public class ServiceKeyUtil
    {
        public static string GetServiceKey(string path)
        {
            return path.ToLower();
        }
    }
}