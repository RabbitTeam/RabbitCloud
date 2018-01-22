using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Abstractions.Codec
{
    public interface IEncoder
    {
        Task EncodeAsync(object instance, Type type, RequestContext requestContext);
    }
}