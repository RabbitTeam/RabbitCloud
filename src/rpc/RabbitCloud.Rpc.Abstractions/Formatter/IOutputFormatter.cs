namespace RabbitCloud.Rpc.Abstractions.Formatter
{
    public interface IOutputFormatter<in T>
    {
        byte[] Format(T instance);
    }
}