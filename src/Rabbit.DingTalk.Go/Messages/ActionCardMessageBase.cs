using Newtonsoft.Json;

namespace Rabbit.DingTalk.Go
{
    public class ActionCardMessageBase : DingTalkMessage
    {
        public ActionCardMessageBase(string title, string text)
        {
            Title = title;
            Text = text;
        }

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.ActionCard;

        #endregion Overrides of DingTalkMessage

        public string Title { get; }
        public string Text { get; }
        public Orientation BtnOrientation { get; set; }

        [JsonConverter(typeof(BoolToIntConverter))]
        public bool HideAvatar { get; set; }
    }
}