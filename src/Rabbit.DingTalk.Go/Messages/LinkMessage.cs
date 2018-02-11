namespace Rabbit.DingTalk.Go
{
    public class LinkMessage : DingTalkMessage
    {
        public LinkMessage(string title, string text, string messageUrl, string picUrl = null)
        {
            Title = title;
            Text = text;
            MessageUrl = messageUrl;
            PicUrl = picUrl;
        }

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.Link;

        #endregion Overrides of DingTalkMessage

        public string Title { get; set; }

        public string Text { get; set; }

        public string MessageUrl { get; set; }

        public string PicUrl { get; set; }
    }
}