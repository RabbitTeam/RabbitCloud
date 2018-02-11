using Newtonsoft.Json;
using System.Collections.Generic;

namespace Rabbit.DingTalk.Go
{
    public class FeedCardMessage : DingTalkMessage
    {
        public class FeedCardItem
        {
            public FeedCardItem(string title, string messageUrl, string picUrl)
            {
                PicUrl = picUrl;
                MessageUrl = messageUrl;
                Title = title;
            }

            public string Title { get; }

            [JsonProperty("messageURL")]
            public string MessageUrl { get; }

            [JsonProperty("PicURL")]
            public string PicUrl { get; }
        }

        public FeedCardMessage()
        {
            Items = new List<FeedCardItem>();
        }

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.FeedCard;

        #endregion Overrides of DingTalkMessage

        [JsonProperty("links")]
        public IList<FeedCardItem> Items { get; }
    }
}