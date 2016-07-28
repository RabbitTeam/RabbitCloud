using Rabbit.Rpc.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace WeChatDistribution.RpcAbstractions
{
    public class WeChatMessageModel
    {
        public WeChatMessageModel(WeChatMessageModel model)
        {
            Id = model.Id;
            ShellName = model.ShellName;
            Content = model.Content;
        }

        public WeChatMessageModel()
        {
        }

        public string ShellName { get; set; }
        public string Id { get; set; }
        public string Content { get; set; }
    }

    [RpcService]
    public interface IMessageHandler
    {
        Task HandleAsync(WeChatMessageModel model);
    }
}