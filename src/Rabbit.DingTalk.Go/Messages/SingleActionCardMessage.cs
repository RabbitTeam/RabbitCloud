using Newtonsoft.Json;

namespace Rabbit.DingTalk.Go
{
    public class SingleActionCardMessage : ActionCardMessageBase
    {
        public SingleActionCardMessage(string title, string text, string singleTitle, string singleUrl) : base(title, text)
        {
            SingleTitle = singleTitle;
            SingleUrl = singleUrl;
        }

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.ActionCard;

        #endregion Overrides of DingTalkMessage

        public string SingleTitle { get; }

        [JsonProperty("SingleURL")]
        public string SingleUrl { get; }
    }
}