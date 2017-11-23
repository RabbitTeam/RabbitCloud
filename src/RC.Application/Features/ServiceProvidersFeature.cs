using System;

namespace Rabbit.Cloud.Application.Features
{
    public class ServiceProvidersFeature : IServiceProvidersFeature
    {
        #region Implementation of IServiceProvidersFeature

        public IServiceProvider RequestServices { get; set; }

        #endregion Implementation of IServiceProvidersFeature
    }
}