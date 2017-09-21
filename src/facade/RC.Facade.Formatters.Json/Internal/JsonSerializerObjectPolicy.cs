using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace Rabbit.Cloud.Facade.Formatters.Json.Internal
{
    public class JsonSerializerObjectPolicy : IPooledObjectPolicy<JsonSerializer>
    {
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonSerializerObjectPolicy"/>.
        /// </summary>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/> used to instantiate
        /// <see cref="JsonSerializer"/> instances.</param>
        public JsonSerializerObjectPolicy(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        /// <inheritdoc />
        public JsonSerializer Create() => JsonSerializer.Create(_serializerSettings);

        /// <inheritdoc />
        public bool Return(JsonSerializer serializer) => true;
    }
}