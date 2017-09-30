using System.IO;

namespace Rabbit.Cloud.Client.Features
{
    public interface IResponseFeature
    {
        int StatusCode { get; set; }
        Stream Body { get; set; }
        bool HasStarted { get; }
    }
}