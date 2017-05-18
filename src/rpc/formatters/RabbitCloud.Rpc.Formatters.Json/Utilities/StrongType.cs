using Newtonsoft.Json.Linq;
using System;

namespace RabbitCloud.Rpc.Formatters.Json.Utilities
{
    public struct StrongType
    {
        public static StrongType? CreateStrongType(object value)
        {
            if (value == null)
                return null;
            var valueType = value.GetType();
            return new StrongType
            {
                TypeName = Type.GetTypeCode(valueType) == TypeCode.Object ? valueType.AssemblyQualifiedName : valueType.FullName,
                Data = value
            };
        }

        public static object GetStrongType(JToken token)
        {
            var typeName = token?.SelectToken("TypeName")?.Value<string>();

            if (string.IsNullOrEmpty(typeName))
                return null;

            var type = Type.GetType(typeName);

            if (type == null)
                throw new NotSupportedException($"not supported type '{typeName}'.");

            return token.SelectToken("Data")?.ToObject(type);
        }

        public string TypeName { get; set; }
        public object Data { get; set; }
    }
}