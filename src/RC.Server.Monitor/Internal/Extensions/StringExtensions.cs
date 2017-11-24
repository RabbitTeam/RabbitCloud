namespace Rabbit.Cloud.Server.Monitor.Internal.Extensions
{
    internal static class StringExtensions
    {
        internal static bool IsPresent(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}