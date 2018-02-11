namespace Rabbit.DingTalk.Go
{
    public class TextMessage : DingTalkMessage, IAtMessage
    {
        public TextMessage(string content)
        {
            Content = content;
        }

        public string Content { get; }

        #region Implementation of IAtMessage

        public string[] AtMobiles { get; set; }
        public bool IsAtAll { get; set; }

        #endregion Implementation of IAtMessage

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.Text;

        #endregion Overrides of DingTalkMessage
    }
}