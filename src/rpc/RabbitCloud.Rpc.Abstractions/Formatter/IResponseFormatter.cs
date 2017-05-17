namespace RabbitCloud.Rpc.Abstractions.Formatter
{
    public interface IResponseFormatter
    {
        IInputFormatter<IResponse> InputFormatter { get; }
        IOutputFormatter<IResponse> OutputFormatter { get; }
    }
}