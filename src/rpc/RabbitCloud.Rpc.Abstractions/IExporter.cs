namespace RabbitCloud.Rpc.Abstractions
{
    public interface IExporter
    {
        ICaller Export();
    }
}