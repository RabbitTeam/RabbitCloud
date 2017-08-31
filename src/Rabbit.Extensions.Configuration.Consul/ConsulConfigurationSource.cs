using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

namespace Rabbit.Extensions.Configuration.Consul
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        public ConsulClient ConsulClient { get; set; }
        public string Path { get; set; }
        public Func<string, string> KeyConver { get; set; }
        public Func<byte[], string> ValueConver { get; set; }
        public bool Optional { get; set; } = true;
        public bool ReloadOnChange { get; set; } = true;

        #region Implementation of IConfigurationSource

        /// <inheritdoc />
        /// <summary>
        /// Builds the <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="T:Microsoft.Extensions.Configuration.IConfigurationBuilder" />.</param>
        /// <returns>An <see cref="T:Microsoft.Extensions.Configuration.IConfigurationProvider" /></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults();
            return new ConsulConfigurationProvider(this);
        }

        #endregion Implementation of IConfigurationSource

        public void EnsureDefaults()
        {
            if (KeyConver == null)
                KeyConver = key => key.Replace("/", ConfigurationPath.KeyDelimiter);
            if (ValueConver == null)
                ValueConver = bytes => bytes == null ? null : Encoding.UTF8.GetString(bytes);

            if (string.IsNullOrWhiteSpace(Path))
                Path = "/";
        }
    }
}