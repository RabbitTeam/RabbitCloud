using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Codec
{
    public interface IEncoder
    {
        Task EncodeAsync(object instance, Type type, GoRequest request);
    }
}