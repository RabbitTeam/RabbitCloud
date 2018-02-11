using System.Threading.Tasks;

namespace Rabbit.DingTalk.Go
{
    public static class DingTalkGoClientExtensions
    {
        public static Task<DingTalkApiResult> SendAsync(this IDingTalkGoClient dingTalkGoClient, DingTalkMessage message)
        {
            return dingTalkGoClient.SendAsync(message, null);
        }

        public static Task<DingTalkApiResult> SendTextAsync(this IDingTalkGoClient client, string text, bool isAtAll = false)
        {
            return client.SendTextAsync(new TextMessage(text) { IsAtAll = isAtAll });
        }

        public static Task<DingTalkApiResult> SendTextAsync(this IDingTalkGoClient client, string text, string[] atMobiles)
        {
            return client.SendTextAsync(new TextMessage(text) { AtMobiles = atMobiles });
        }

        public static Task<DingTalkApiResult> SendTextAsync(this IDingTalkGoClient client, TextMessage message)
        {
            return client.SendAsync(message);
        }

        public static Task<DingTalkApiResult> SendActionCardAsync(this IDingTalkGoClient client, ActionCardMessage message)
        {
            return client.SendAsync(message);
        }

        public static Task<DingTalkApiResult> SendFeedCardAsync(this IDingTalkGoClient client, FeedCardMessage message)
        {
            return client.SendAsync(message);
        }

        public static Task<DingTalkApiResult> SendLinkAsync(this IDingTalkGoClient client, LinkMessage message)
        {
            return client.SendAsync(message);
        }

        public static Task<DingTalkApiResult> SendMarkdownAsync(this IDingTalkGoClient client, string title, string text, bool isAtAll = false)
        {
            return client.SendMarkdownAsync(new MarkdownMessage(title, text) { IsAtAll = isAtAll });
        }

        public static Task<DingTalkApiResult> SendMarkdownAsync(this IDingTalkGoClient client, string title, string text, string[] atMobiles)
        {
            return client.SendMarkdownAsync(new MarkdownMessage(title, text) { AtMobiles = atMobiles });
        }

        public static Task<DingTalkApiResult> SendMarkdownAsync(this IDingTalkGoClient client, MarkdownMessage message)
        {
            return client.SendAsync(message);
        }

        public static Task<DingTalkApiResult> SendSingleActionCardAsync(this IDingTalkGoClient client, SingleActionCardMessage message)
        {
            return client.SendAsync(message);
        }
    }
}