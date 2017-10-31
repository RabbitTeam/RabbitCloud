using Grpc.Core;

namespace Rabbit.Cloud.Grpc.Client
{
    public interface IChannelLocator
    {
        Channel Locate(string serviceId);
    }
}