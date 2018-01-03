namespace Rabbit.Cloud.Client.Abstractions.Codec
{
    public interface ICodec
    {
        object Encode(object body);

        object Decode(object data);
    }
}
