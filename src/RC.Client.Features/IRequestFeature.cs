using System.IO;

namespace Rabbit.Cloud.Client.Features
{
    public interface IRequestFeature
    {
        string ServiceName { get; set; }
        string Scheme { get; set; }
        string Path { get; set; }
        string QueryString { get; set; }
        Stream Body { get; set; }
    }
}