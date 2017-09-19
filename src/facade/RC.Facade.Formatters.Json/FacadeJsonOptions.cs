using Newtonsoft.Json;

namespace RC.Facade.Formatters.Json
{
    public class FacadeJsonOptions
    {
        public JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings();
    }
}