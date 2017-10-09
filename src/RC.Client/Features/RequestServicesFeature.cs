using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace Rabbit.Cloud.Client.Features
{
    public class RequestServicesFeature : IServiceProvidersFeature, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IServiceProvider _requestServices;
        private IServiceScope _scope;
        private bool _requestServicesSet;

        public RequestServicesFeature(IServiceScopeFactory scopeFactory)
        {
            Debug.Assert(scopeFactory != null);
            _scopeFactory = scopeFactory;
        }

        #region Implementation of IServiceProvidersFeature

        public IServiceProvider RequestServices
        {
            get
            {
                if (_requestServicesSet)
                    return _requestServices;
                _scope = _scopeFactory.CreateScope();
                _requestServices = _scope.ServiceProvider;
                _requestServicesSet = true;
                return _requestServices;
            }

            set
            {
                _requestServices = value;
                _requestServicesSet = true;
            }
        }

        #endregion Implementation of IServiceProvidersFeature

        #region Implementation of IDisposable

        /// <inheritdoc />
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _scope?.Dispose();
            _scope = null;
            _requestServices = null;
        }

        #endregion Implementation of IDisposable
    }
}