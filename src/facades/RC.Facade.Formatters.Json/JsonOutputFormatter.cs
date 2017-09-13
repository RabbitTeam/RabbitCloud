using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using RC.Facade.Formatters.Json.Internal;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOutputFormatter = Rabbit.Cloud.Facade.Abstractions.Formatters.IOutputFormatter;

namespace RC.Facade.Formatters.Json
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        protected JsonSerializerSettings SerializerSettings { get; }
        private readonly Internal.JsonArrayPool<char> _charPool;
        private readonly ObjectPoolProvider _objectPoolProvider;
        private readonly ILogger<JsonOutputFormatter> _logger;
        private ObjectPool<JsonSerializer> _jsonSerializerPool;

        public JsonOutputFormatter(JsonSerializerSettings jsonSerializerSettings, ArrayPool<char> charPool, ObjectPoolProvider objectPoolProvider, ILogger<JsonOutputFormatter> logger)
        {
            SerializerSettings = jsonSerializerSettings;
            _charPool = new Internal.JsonArrayPool<char>(charPool);
            _objectPoolProvider = objectPoolProvider;
            _logger = logger;
        }

        #region Implementation of IOutputFormatter

        public bool CanWriteResult(OutputFormatterContext context)
        {
            var response = context.RabbitContext.Response.ResponseMessage;
            var contentType = response.Content.Headers.ContentType;

            if (contentType == null)
                return false;

            return new[] { "application/json", "text/json" }.Contains(contentType.MediaType,
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<OutputFormatterResult> WriteAsync(OutputFormatterContext context)
        {
            using (var reader = new StreamReader(context.Stream))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jsonReader.ArrayPool = _charPool;
                    jsonReader.CloseInput = false;

                    var successful = true;

                    void ErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs eventArgs)
                    {
                        successful = false;

                        _logger.JsonInputException(eventArgs.ErrorContext.Error);

                        // Error must always be marked as handled
                        // Failure to do so can cause the exception to be rethrown at every recursive level and
                        // overflow the stack for x64 CLR processes
                        eventArgs.ErrorContext.Handled = true;
                    }

                    var type = context.ModelType;
                    var jsonSerializer = CreateJsonSerializer();
                    jsonSerializer.Error += ErrorHandler;
                    object model;
                    try
                    {
                        model = jsonSerializer.Deserialize(jsonReader, type);
                    }
                    finally
                    {
                        jsonSerializer.Error -= ErrorHandler;
                        ReleaseJsonSerializer(jsonSerializer);
                    }
                    if (!successful)
                        return OutputFormatterResult.Failure();
                    if (model == null && !context.TreatEmptyInputAsDefaultValue)
                    {
                        return await OutputFormatterResult.NoValueAsync();
                    }
                    return OutputFormatterResult.Success(model);
                }
            }
        }

        #endregion Implementation of IOutputFormatter

        private JsonSerializer CreateJsonSerializer()

        {
            if (_jsonSerializerPool == null)
            {
                _jsonSerializerPool = _objectPoolProvider.Create(new JsonSerializerObjectPolicy(SerializerSettings));
            }

            return _jsonSerializerPool.Get();
        }

        protected virtual void ReleaseJsonSerializer(JsonSerializer serializer)
            => _jsonSerializerPool.Return(serializer);
    }
}