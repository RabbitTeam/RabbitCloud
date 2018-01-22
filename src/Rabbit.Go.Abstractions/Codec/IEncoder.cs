using System;

namespace Rabbit.Go.Abstractions.Codec
{
    public interface IEncoder
    {
        void Encode(object instance, Type type, RequestContext requestContext);
    }
}