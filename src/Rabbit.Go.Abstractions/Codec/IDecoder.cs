using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Go.Codec
{
    public interface IDecoder
    {
        Task<object> DecodeAsync(HttpResponseMessage response, Type type);
    }
}