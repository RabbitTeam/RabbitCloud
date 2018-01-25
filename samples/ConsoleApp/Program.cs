using Rabbit.DingTalk.Go;
using Rabbit.Go;
using Rabbit.Go.Interceptors;
using System;
using System.Threading.Tasks;
using ActionCardButton = Rabbit.DingTalk.Go.ActionCardMessage.ActionCardButton;
using FeedCardItem = Rabbit.DingTalk.Go.FeedCardMessage.FeedCardItem;

namespace ConsoleApp
{
    /*    public class MyClass2:DecoderAttribute
        {
            #region Overrides of DecoderAttribute

            public override Task<object> DecodeAsync(HttpResponseMessage response, Type type)
            {
                return base.DecodeAsync(response, type);
            }

            #endregion Overrides of DecoderAttribute
        }
*/

    internal class MyClass : RequestInterceptorAttribute
    {
        #region Overrides of RequestInterceptorAttribute

        public override void OnRequestExecuting(RequestExecutingContext context)
        {
            Console.WriteLine("OnRequestExecuting");
        }

        public override void OnRequestExecuted(RequestExecutedContext context)
        {
            base.OnRequestExecuted(context);
        }

        #endregion Overrides of RequestInterceptorAttribute
    }

    [Go("http://192.168.100.150:7704/{userId}")]
    public interface ITestGoClient
    {
        [GoGet("/gift/count")]
        Task<long> TestAsync(long userId);
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var dingTalk = new GoBuilder()
                .Codec(new DingTalkCodec())
                .Target<IDingTalkGoClient>();

            await dingTalk.SendAsync(new TextMessage("textMessage"),
                "049f81883f1c4896fc7d65423d8608607c31d6c922c0c1ff5f4453dc3a81cd04");
            return;
            dingTalk = new GoBuilder()
                .Query("access_token", "049f81883f1c4896fc7d65423d8608607c31d6c922c0c1ff5f4453dc3a81cd04")
                .Codec(new DingTalkCodec())
                .Target<IDingTalkGoClient>();

            await dingTalk.SendAsync(new TextMessage("test"));
            await dingTalk.SendAsync(new LinkMessage("title", "text", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            await dingTalk.SendAsync(new MarkdownMessage("杭州天气", "#### 杭州天气 @156xxxx8827")
            {
                IsAtAll = true
            });
            await dingTalk.SendAsync(
                new SingleActionCardMessage("title",
                    "text",
                    "sTitle",
                    "https://open-doc.dingtalk.com/")
                {
                    BtnOrientation = Orientation.Vertical,
                    HideAvatar = false
                });

            var message = new ActionCardMessage("乔布斯 20 年前想打造一间苹果咖啡厅，而它正是 Apple Store 的前身", @"![screenshot](@lADOpwk3K80C0M0FoA)
            ### 乔布斯 20 年前想打造的苹果咖啡厅
                        Apple Store 的设计正从原来满满的科技感走向生活化，而其生活化的走向其实可以追溯到 20 年前苹果一个建立咖啡馆的计划")
            {
                HideAvatar = true,
                BtnOrientation = Orientation.Horizontal
            };

            message.Buttons.Add(new ActionCardButton("内容不错", "https://www.dingtalk.com/"));
            message.Buttons.Add(new ActionCardButton("不感兴趣", "https://www.dingtalk.com/"));
            await dingTalk.SendAsync(message);

            var feedCardMessage = new FeedCardMessage();
            feedCardMessage.Items.Add(new FeedCardItem("title1", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            feedCardMessage.Items.Add(new FeedCardItem("title2", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            feedCardMessage.Items.Add(new FeedCardItem("title3", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));

            await dingTalk.SendAsync(feedCardMessage);
        }
    }
}