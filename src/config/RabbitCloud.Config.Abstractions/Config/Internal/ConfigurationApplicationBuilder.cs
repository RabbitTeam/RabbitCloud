using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RabbitCloud.Config.Abstractions.Config.Internal
{
    public class ConfigurationApplicationBuilder : IApplicationBuilder
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private static string _localHost;

        public ConfigurationApplicationBuilder(IServiceProvider services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
        }

        #region Implementation of IApplicationBuilder

        public async Task<ApplicationEntry> Build()
        {
            var defaultConfig = _configuration.GetSection("Default");
            var defaultProtocolConfig = new ProtocolConfig();
            var defaultRegistryConfig = new RegistryConfig();
            defaultConfig.GetSection("Protocol").Bind(defaultProtocolConfig);
            defaultConfig.GetSection("Registry").Bind(defaultRegistryConfig);

            if (defaultProtocolConfig.Host == null)
                defaultProtocolConfig.Host = await GetLocalHost();

            var applicationConfig = new ApplicationConfig();
            _configuration.Bind(applicationConfig);

            var serviceExportConfigs = new List<ServiceExportConfig>();
            var referenceConfigs = new List<ReferenceConfig>();

            foreach (var section in _configuration.GetSection("Exports").GetChildren())
            {
                var exportConfig = GetServiceExportConfig(section, defaultProtocolConfig, defaultRegistryConfig);
                serviceExportConfigs.Add(exportConfig);
            }
            foreach (var section in _configuration.GetSection("References").GetChildren())
            {
                var referenceConfig = GetReferenceConfig(section, defaultProtocolConfig, defaultRegistryConfig);
                referenceConfigs.Add(referenceConfig);
            }

            applicationConfig.ServiceExportConfigs = serviceExportConfigs.ToArray();
            applicationConfig.ReferenceConfigs = referenceConfigs.ToArray();

            var builder = new ApplicationBuilder(_services, applicationConfig);
            return await builder.Build();
        }

        #endregion Implementation of IApplicationBuilder

        private static ProtocolConfig GetProtocolConfig(IConfigurationSection section, ProtocolConfig defaultProtocolConfig)
        {
            if (section.Value == null)
                return defaultProtocolConfig;

            var config = new ProtocolConfig();
            section.Bind(config);

            if (config.Host == null)
                config.Host = defaultProtocolConfig.Host;

            return config;
        }

        private static RegistryConfig GetRegistryConfig(IConfigurationSection section, RegistryConfig defaultRegistryConfig)
        {
            if (section.Value == null)
                return defaultRegistryConfig;

            var config = new RegistryConfig();
            section.Bind(config);
            return config;
        }

        private static ServiceConfig GetServiceConfig(IConfiguration section)
        {
            var config = new ServiceConfig();
            section.Bind(config);
            return config;
        }

        private static ServiceExportConfig GetServiceExportConfig(IConfiguration configuration, ProtocolConfig defaultProtocolConfig, RegistryConfig defaultRegistryConfig)
        {
            var config = new ServiceExportConfig();
            configuration.Bind(config);

            var protocolSection = configuration.GetSection("Protocol");
            var registrySection = configuration.GetSection("Registry");

            config.ProtocolConfig = GetProtocolConfig(protocolSection, defaultProtocolConfig);
            config.RegistryConfig = GetRegistryConfig(registrySection, defaultRegistryConfig);
            config.ServiceConfig = GetServiceConfig(configuration);

            return config;
        }

        private static ReferenceConfig GetReferenceConfig(IConfiguration configuration, ProtocolConfig defaultProtocolConfig, RegistryConfig defaultRegistryConfig)
        {
            var config = new ReferenceConfig();
            configuration.Bind(config);

            if (config.Id == null)
                config.Id = Type.GetType(config.InterfaceType).Name;

            var protocolSection = configuration.GetSection("Protocol");
            var registrySection = configuration.GetSection("Registry");

            config.ProtocolConfig = GetProtocolConfig(protocolSection, defaultProtocolConfig);
            config.RegistryConfig = GetRegistryConfig(registrySection, defaultRegistryConfig);

            return config;
        }

        private static async Task<string> GetLocalHost()
        {
            if (_localHost != null)
                return _localHost;
            var address = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            var ip4Address = address.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
            return _localHost = ip4Address.ToString();
        }
    }
}