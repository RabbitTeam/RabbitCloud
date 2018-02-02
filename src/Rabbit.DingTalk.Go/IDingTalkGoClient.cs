using Rabbit.Go;
using System.Threading.Tasks;

namespace Rabbit.DingTalk.Go
{
    [Go("https://oapi.dingtalk.com/robot/send"), DingTalkCodec]
    public interface IDingTalkGoClient
    {
        [GoPost]
        Task SendAsync([GoBody]DingTalkMessage message);

        [GoPost]
        Task SendAsync([GoBody]DingTalkMessage message, [GoQuery("access_token")]string accessToken);
    }
}