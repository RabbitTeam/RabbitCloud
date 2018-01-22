using Microsoft.Extensions.DependencyInjection;
using Rabbit.DingTalk.Go;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Core;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
/*            var appServices=new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .BuildServiceProvider();

            var app=new RabbitApplicationBuilder(appServices)
                .UseRabbitClient()
                .UseRabbitHttpClient()
                .Build();*/

            var dingTalkCodec = new DingTalkCodec();
            var services=new ServiceCollection()
                .AddOptions()
                .Configure<GoOptions>(o =>
                    {
                        o.FormatterMappings.Set("application/json", dingTalkCodec, dingTalkCodec);
                    })
                .AddGo()
                .BuildServiceProvider();

            var goFactory = services.GetService<IGoFactory>();

            var dingTalkGoClient = goFactory.CreateInstance<IDingTalkGoClient>();

            //            await dingTalkGoClient.SendAsync(new TextMessage("test"));
            //            await dingTalkGoClient.SendAsync(new LinkMessage("title","text","http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            //            return;
            //            await dingTalkGoClient.SendAsync(new MarkdownMessage("杭州天气", "#### 杭州天气 @156xxxx8827") {IsAtAll = true});
            //            return;
            await dingTalkGoClient.SendAsync(
                new SingleActionCardMessage("title", "text", "sTitle", "https://open-doc.dingtalk.com/")
                {
                    BtnOrientation = Orientation.Vertical,
                    HideAvatar = false
                });
            return;
            /*

                        var message = new ActionCardMessage("乔布斯 20 年前想打造一间苹果咖啡厅，而它正是 Apple Store 的前身", @"![screenshot](@lADOpwk3K80C0M0FoA) 
            ### 乔布斯 20 年前想打造的苹果咖啡厅 
                        Apple Store 的设计正从原来满满的科技感走向生活化，而其生活化的走向其实可以追溯到 20 年前苹果一个建立咖啡馆的计划")
                        {
                            HideAvatar = true,
                            BtnOrientation = Orientation.Horizontal
                        };

                        message.Buttons.Add(new ActionCardMessage.ActionCardButton("内容不错", "https://www.dingtalk.com/"));
                        message.Buttons.Add(new ActionCardMessage.ActionCardButton("不感兴趣", "https://www.dingtalk.com/"));
            */

            var message = new FeedCardMessage();
            message.Items.Add(new FeedCardMessage.FeedCardItem("title1","http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            message.Items.Add(new FeedCardMessage.FeedCardItem("title2", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));
            message.Items.Add(new FeedCardMessage.FeedCardItem("title3", "http://www.baidu.com", "https://www.baidu.com/img/bd_logo1.png"));

            await dingTalkGoClient.SendAsync(message);

        }
    }
}