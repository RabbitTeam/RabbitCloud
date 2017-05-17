namespace RabbitCloud.Rpc.Abstractions.Formatter
{
    public interface IRequestFormatter
    {
        IInputFormatter<IRequest> InputFormatter { get; }
        IOutputFormatter<IRequest> OutputFormatter { get; }
    }
}