using Rabbit.Cloud.Client.Abstractions.Codec;
using Rabbit.Cloud.Client.Abstractions.Features;

namespace Rabbit.Cloud.Client.Features
{
    public class CodecFeature : ICodecFeature
    {
        #region Implementation of ICodecFeature

        public ICodec Codec { get; set; }

        #endregion Implementation of ICodecFeature
    }
}