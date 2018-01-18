using Rabbit.Cloud.Client.Abstractions.Codec;

namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface ICodecFeature
    {
        ICodec Codec { get; set; }
    }
}