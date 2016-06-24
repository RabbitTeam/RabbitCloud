namespace Rabbit.Rpc.Transport
{
    /// <summary>
    /// 一个抽象的消息通道。
    /// </summary>
    public interface IMessageChannel : IMessageSender, IMessageListener
    {
    }
}