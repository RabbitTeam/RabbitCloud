using Newtonsoft.Json;

namespace Rabbit.DingTalk.Go
{
    public interface IAtMessage
    {
        [JsonIgnore]
        string[] AtMobiles { get; set; }

        [JsonIgnore]
        bool IsAtAll { get; set; }
    }
}