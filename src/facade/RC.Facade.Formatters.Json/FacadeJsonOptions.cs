using Newtonsoft.Json;

namespace Rabbit.Cloud.Facade.Formatters.Json
{
    public class FacadeJsonOptions
    {
        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings();
    }
}