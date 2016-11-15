namespace RabbitCloud.Rpc.Abstractions.Features
{
    public interface ICodecFeature
    {
        ICodec Codec { get; set; }
    }

    public class CodecFeature : ICodecFeature
    {
        #region Implementation of ICodecFeature

        public ICodec Codec { get; set; }

        #endregion Implementation of ICodecFeature
    }
}