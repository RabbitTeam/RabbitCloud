using RabbitCloud.Config.Abstractions.Adapter;
using RabbitCloud.Rpc.Abstractions.Formatter;

namespace RabbitCloud.Rpc.Formatters.Json.Config
{
    public class JsonFormatterProvider : IFormatterProvider
    {
        #region Implementation of IFormatterProvider

        public string Name { get; } = "json";

        public IRequestFormatter CreateRequestFormatter()
        {
            return new JsonRequestFormatter();
        }

        public IResponseFormatter CreateResponseFormatter()
        {
            return new JsonResponseFormatter();
        }

        #endregion Implementation of IFormatterProvider
    }
}