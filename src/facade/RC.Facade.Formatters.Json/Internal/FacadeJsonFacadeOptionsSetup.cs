using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rabbit.Cloud.Facade;
using System.Buffers;

namespace RC.Facade.Formatters.Json.Internal
{
    public class FacadeJsonFacadeOptionsSetup : IConfigureOptions<FacadeOptions>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ArrayPool<char> _charPool;
        private readonly ObjectPoolProvider _objectPoolProvider;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public FacadeJsonFacadeOptionsSetup(
            ILoggerFactory loggerFactory,
            IOptions<FacadeJsonOptions> jsonOptions,
            ArrayPool<char> charPool,
            ObjectPoolProvider objectPoolProvider)
        {
            _loggerFactory = loggerFactory;
            _charPool = charPool;
            _objectPoolProvider = objectPoolProvider;
            _jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
        }

        #region Implementation of IConfigureOptions<in FacadeOptions>

        /// <inheritdoc />
        /// <summary>Invoked to configure a TOptions instance.</summary>
        /// <param name="options">The options instance to configure.</param>
        public void Configure(FacadeOptions options)
        {
            options.OutputFormatters.Add(new JsonOutputFormatter(_jsonSerializerSettings, _charPool, _objectPoolProvider, _loggerFactory.CreateLogger<JsonOutputFormatter>()));
            options.InputFormatters.Add(new JsonInputFormatter(_jsonSerializerSettings, _charPool));
        }

        #endregion Implementation of IConfigureOptions<in FacadeOptions>
    }
}