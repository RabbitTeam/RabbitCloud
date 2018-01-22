using System;
using System.Net.Http;

namespace Rabbit.Go.Abstractions.Codec
{
    public interface IDecoder
    {
        object Decode(HttpResponseMessage response, Type type);
    }
}