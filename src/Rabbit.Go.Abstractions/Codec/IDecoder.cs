using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Codec
{
    public interface IDecoder
    {
        Task<object> DecodeAsync(GoResponse response, Type type);
    }
}