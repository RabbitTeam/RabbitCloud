namespace RabbitCloud.Rpc.Abstractions.Formatter
{
    public interface IInputFormatter<out T>
    {
        T Format(byte[] data);
    }
}