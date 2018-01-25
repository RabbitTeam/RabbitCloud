// ReSharper disable once CheckNamespace
namespace System.Net.Http
{
    public static class HttpRequestMessageExtensions
    {
        public static T GetProperty<T>(HttpRequestMessage requestMessage, string key)
        {
            return requestMessage.Properties.TryGetValue(key, out var value) ? (T)value : default(T);
        }

        public static HttpRequestMessage SetProperty<T>(this HttpRequestMessage requestMessage, string key, T value)
        {
            requestMessage.Properties[key] = value;
            return requestMessage;
        }

        public static bool TrySetProperty<T>(this HttpRequestMessage requestMessage, string key, T value)
        {
            if (requestMessage.Properties.ContainsKey(key))
                return false;

            requestMessage.SetProperty(key, value);

            return true;
        }
    }
}