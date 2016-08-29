namespace Cowboy.Sockets.Tcp.Framing
{
    public interface IFrameEncoder
    {
        void EncodeFrame(byte[] payload, int offset, int count, out byte[] frameBuffer, out int frameBufferOffset, out int frameBufferLength);
    }
}
