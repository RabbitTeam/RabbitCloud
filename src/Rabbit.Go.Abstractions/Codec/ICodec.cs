using Rabbit.Go.Codec;

namespace Rabbit.Go.Abstractions.Codec
{
    public interface ICodec
    {
        IEncoder Encoder { get; }
        IDecoder Decoder { get; }
    }
}