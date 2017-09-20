using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Abstractions.Features;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Features;
using Rabbit.Cloud.Internal;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Internal
{
    public class RabbitCloudClient : IRabbitCloudClient
    {
        private readonly Func<IServiceCollection, IServiceProvider, IServiceProvider> _configureServices;
        private readonly Action<IRabbitApplicationBuilder> _configure;
        private readonly IServiceCollection _applicationServiceCollection;
        private readonly IServiceProvider _hostingServiceProvider;
        private IServiceProvider _applicationServices;
        private RabbitRequestDelegate _application;

        public RabbitCloudClient(IServiceCollection appServices, IServiceProvider hostingServiceProvider, Func<IServiceCollection, IServiceProvider, IServiceProvider> configureServices, Action<IRabbitApplicationBuilder> configure)
        {
            _applicationServiceCollection = appServices;
            _hostingServiceProvider = hostingServiceProvider;

            _configureServices = configureServices;
            _configure = configure;
            ServerFeatures = new FeatureCollection();
        }

        #region Implementation of IRabbitCloudClient

        public IFeatureCollection ServerFeatures { get; }

        public IServiceProvider Services => _applicationServices ??
                                            (_applicationServices = _configureServices(_applicationServiceCollection, _hostingServiceProvider));

        public async Task RequestAsync(RabbitContext context)
        {
            if (_application == null)
                Initialize();

            await _application(context);
        }

        #endregion Implementation of IRabbitCloudClient

        public void Initialize()
        {
            if (_application == null)
            {
                _application = BuildApplication();
            }
        }

        private RabbitRequestDelegate BuildApplication()
        {
            var builder = new RabbitApplicationBuilder(_applicationServices);

            _configure(builder);

            return builder.Build();
        }
    }
}