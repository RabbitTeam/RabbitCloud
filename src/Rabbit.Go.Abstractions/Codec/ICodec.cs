namespace Rabbit.Go.Codec
{
    public interface ICodec
    {
        IEncoder Encoder { get; }
        IDecoder Decoder { get; }
    }
}