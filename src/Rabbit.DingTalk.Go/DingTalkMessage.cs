using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Abstractions.Codec;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.DingTalk.Go
{
    public class DingTalkCodec : IEncoder, IDecoder
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        #region Implementation of IEncoder

        public Task EncodeAsync(object instance, Type type, RequestContext requestContext)
        {
            if (!(instance is DingTalkMessage dingTalkMessage))
                return Task.CompletedTask;

            var messageType = dingTalkMessage.Type.ToString();

            messageType = messageType[0].ToString().ToLower() + messageType.Substring(1);
            var propertyName = messageType;

            var dictionary = new
                Dictionary<string, object>
                {
                    {"msgtype",messageType },
                    {propertyName, instance}
                };

            if (instance is IAtMessage atMessage)
            {
                dictionary["at"] = JsonConvert.SerializeObject(new
                {
                    atMobiles = atMessage.AtMobiles,
                    isAtAll = atMessage.IsAtAll
                });
            }

            var json = JsonConvert.SerializeObject(dictionary, _jsonSerializerSettings);
            requestContext.SetBody(json);

            return Task.CompletedTask;
        }

        #endregion Implementation of IEncoder

        #region Implementation of IDecoder

        public async Task<object> DecodeAsync(HttpResponseMessage response, Type type)
        {
            //            { "errcode":300001,"errmsg":"token is not exist"}
            //            { "errcode":0,"errmsg":"ok"}
            //todo:等待处理
            var t = await response.Content.ReadAsStringAsync();
            return null;
        }

        #endregion Implementation of IDecoder
    }

    public enum MessageType
    {
        Text,
        Link,
        Markdown,
        ActionCard,
        FeedCard
    }

    public interface IAtMessage
    {
        [JsonIgnore]
        string[] AtMobiles { get; set; }

        [JsonIgnore]
        bool IsAtAll { get; set; }
    }

    public abstract class DingTalkMessage
    {
        [JsonIgnore]
        public abstract MessageType Type { get; }
    }

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

    internal class BoolToIntConverter : JsonConverter
    {
        #region Overrides of JsonConverter

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is bool b))
                return;
            writer.WriteValue(b ? "1" : "0");
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }

        #endregion Overrides of JsonConverter
    }

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

    public enum Orientation
    {
        Vertical = 0,
        Horizontal = 1
    }

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