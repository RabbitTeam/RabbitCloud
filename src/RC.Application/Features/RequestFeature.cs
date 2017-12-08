using System;

namespace Rabbit.Cloud.Application.Features
{
    public class RequestFeature : IRequestFeature
    {
        #region Implementation of IRequestFeature

        public ServiceUrl ServiceUrl { get; set; }
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(10);

        #endregion Implementation of IRequestFeature
    }
}