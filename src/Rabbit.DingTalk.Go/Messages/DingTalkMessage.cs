using Newtonsoft.Json;

namespace Rabbit.DingTalk.Go
{
    public abstract class DingTalkMessage
    {
        [JsonIgnore]
        public abstract MessageType Type { get; }
    }
}