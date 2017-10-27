namespace Rabbit.Cloud.Client.Features
{
    public interface IRequestFeature
    {
        string ServiceName { get; set; }
        string Host { get; set; }
        int Port { get; set; }
    }
}