using System;
using System.IO;

namespace Rabbit.Cloud.Client.Serialization
{

    public interface ISerializer
    {
        void Serialize(object instance, Stream stream);

        object Deserialize(Stream stream, Type type);
    }
}
