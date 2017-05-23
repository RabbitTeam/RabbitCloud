using RabbitCloud.Rpc.Abstractions.Formatter;

namespace RabbitCloud.Config.Abstractions.Adapter
{
    public interface IFormatterProvider
    {
        string Name { get; }

        IRequestFormatter CreateRequestFormatter();

        IResponseFormatter CreateResponseFormatter();
    }
}