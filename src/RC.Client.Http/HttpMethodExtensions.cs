using System.Net.Http;

namespace Rabbit.Cloud.Client.Http
{
    public static class HttpMethodExtensions
    {
        public static bool HaveBody(this HttpMethod httpMethod)
        {
            var method = httpMethod.Method.ToLower();
            switch (method)
            {
                case "get":
                case "head":
                case "copy":
                case "purge":
                case "unlock":
                    return false;

                default:
                    return true;
            }
        }

        public static HttpMethod GetHttpMethod(string method, HttpMethod defaultHttpMethod)
        {
            if (string.IsNullOrEmpty(method))
                return defaultHttpMethod;

            switch (method.ToLower())
            {
                case "get":
                    return HttpMethod.Get;

                case "post":
                    return HttpMethod.Post;

                case "put":
                    return HttpMethod.Put;

                case "delete":
                    return HttpMethod.Delete;

                case "head":
                    return HttpMethod.Head;

                case "options":
                    return HttpMethod.Options;

                case "trace":
                    return HttpMethod.Trace;

                default:
                    return new HttpMethod(method);
            }
        }
    }
}