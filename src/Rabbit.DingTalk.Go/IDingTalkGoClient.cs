using Rabbit.Go.Abstractions;
using System.Threading.Tasks;

namespace Rabbit.DingTalk.Go
{
    [Go("https://oapi.dingtalk.com/robot/send?access_token=token")]
    [DefaultHeader("Content-Type", "application/json")]
    public interface IDingTalkGoClient
    {
        [GoPost]
        Task SendAsync([GoBody]TextMessage message);

        [GoPost]
        Task SendAsync([GoBody]LinkMessage message);

        [GoPost]
        Task SendAsync([GoBody]MarkdownMessage message);

        [GoPost]
        Task SendAsync([GoBody]SingleActionCardMessage message);

        [GoPost]
        Task SendAsync([GoBody]ActionCardMessage message);

        [GoPost]
        Task SendAsync([GoBody]FeedCardMessage message);
    }
}