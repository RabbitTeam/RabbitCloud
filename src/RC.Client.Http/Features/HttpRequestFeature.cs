namespace Rabbit.Cloud.Client.Http.Features
{
    public interface IHttpRequestFeature
    {
        string Method { get; set; }
        string ContentType { get; set; }
    }

    public class HttpRequestFeature : IHttpRequestFeature
    {
        #region Implementation of IHttpRequestFeature

        public string Method { get; set; }
        public string ContentType { get; set; }

        #endregion Implementation of IHttpRequestFeature
    }
}