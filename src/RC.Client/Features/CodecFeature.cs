using System;

namespace Rabbit.Cloud.Client.Features
{
    public interface ICodecFeature
    {
        Type RequesType { get; set; }
        Type ResponseType { get; set; }
        ICodec Codec { get; set; }
    }

    public class CodecFeature : ICodecFeature
    {
        #region Implementation of ICodecFeature

        public Type RequesType { get; set; }
        public Type ResponseType { get; set; }
        public ICodec Codec { get; set; }

        #endregion Implementation of ICodecFeature
    }
}