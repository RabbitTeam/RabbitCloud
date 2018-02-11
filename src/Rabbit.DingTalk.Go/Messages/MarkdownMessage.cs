namespace Rabbit.DingTalk.Go
{
    public class MarkdownMessage : DingTalkMessage, IAtMessage
    {
        public MarkdownMessage(string title, string text)
        {
            Title = title;
            Text = text;
        }

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.Markdown;

        #endregion Overrides of DingTalkMessage

        public string Title { get; set; }
        public string Text { get; set; }

        #region Implementation of IAtMessage

        public string[] AtMobiles { get; set; }
        public bool IsAtAll { get; set; }

        #endregion Implementation of IAtMessage
    }
}