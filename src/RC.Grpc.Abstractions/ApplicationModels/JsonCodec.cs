using Newtonsoft.Json;
using System;
using System.Text;

namespace Rabbit.Cloud.Grpc.Abstractions.ApplicationModels
{
    public class JsonCodec : Codec
    {
        protected override byte[] DoEncode(object model)
        {
            var content = JsonConvert.SerializeObject(model);
            return Encoding.UTF8.GetBytes(content);
        }

        protected override object DoDecode(byte[] data, Type type)
        {
            var content = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject(content, type);
        }
    }
}