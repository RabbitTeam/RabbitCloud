using Newtonsoft.Json;
using System.Collections.Generic;

namespace Rabbit.DingTalk.Go
{
    public class ActionCardMessage : ActionCardMessageBase
    {
        public class ActionCardButton
        {
            public ActionCardButton(string title, string actionUrl)
            {
                Title = title;
                ActionUrl = actionUrl;
            }

            public string Title { get; }

            [JsonProperty("ActionURL")]
            public string ActionUrl { get; }
        }

        public ActionCardMessage(string title, string text) : base(title, text)
        {
            Buttons = new List<ActionCardButton>();
        }

        #region Overrides of DingTalkMessage

        public override MessageType Type => MessageType.ActionCard;

        #endregion Overrides of DingTalkMessage

        [JsonProperty("btns")]
        public IList<ActionCardButton> Buttons { get; }
    }
}