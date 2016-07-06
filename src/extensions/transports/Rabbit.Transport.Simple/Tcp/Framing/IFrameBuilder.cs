namespace Rabbit.Transport.Simple.Tcp.Framing
{
    public interface IFrameBuilder
    {
        IFrameEncoder Encoder { get; }
        IFrameDecoder Decoder { get; }
    }
}
