using System;

namespace Rabbit.Cloud.Application.Features
{
    public class ServiceUrl
    {
        public ServiceUrl(string url)
        {
            var uri = new Uri(url);

            Scheme = uri.Scheme;
            Host = uri.Host;
            Port = uri.Port;
            Path = uri.AbsolutePath;
        }

        public ServiceUrl(ServiceUrl serviceUrl)
        {
            Scheme = serviceUrl.Scheme;
            Host = serviceUrl.Host;
            Port = serviceUrl.Port;
            Path = serviceUrl.Path;
        }

        public ServiceUrl()
        {
        }

        private string _scheme;

        public string Scheme
        {
            get => _scheme;
            set => _scheme = value?.ToLower();
        }

        private string _host;

        public string Host
        {
            get => _host;
            set => _host = value?.ToLower();
        }

        public int Port { get; set; }
        public string Path { get; set; }

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Scheme}://{Host}:{(Port <= 0 ? Path : Port + Path)}";
        }

        #endregion Overrides of Object
    }

    public interface IRequestFeature
    {
        ServiceUrl ServiceUrl { get; set; }
        TimeSpan ConnectionTimeout { get; set; }
        TimeSpan ReadTimeout { get; set; }
    }
}