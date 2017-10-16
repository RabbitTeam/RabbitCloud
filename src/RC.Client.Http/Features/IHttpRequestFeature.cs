using Rabbit.Cloud.Client.Features;
using System.Net.Http;

namespace Rabbit.Cloud.Client.Http.Features
{
    public interface IHttpRequestFeature : IRequestFeature
    {
        HttpMethod Method { get; set; }
    }
}