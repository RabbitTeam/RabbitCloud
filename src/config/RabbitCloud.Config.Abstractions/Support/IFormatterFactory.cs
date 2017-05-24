using RabbitCloud.Rpc.Abstractions.Formatter;

namespace RabbitCloud.Config.Abstractions.Support
{
    public interface IFormatterFactory
    {
        IRequestFormatter GetRequestFormatter(string name);

        IResponseFormatter GetResponseFormatter(string name);
    }
}