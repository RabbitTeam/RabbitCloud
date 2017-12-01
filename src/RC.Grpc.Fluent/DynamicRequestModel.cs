using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Fluent
{
    internal class DynamicRequestModel
    {
        public IDictionary<string, byte[]> Items { get; set; }
    }

    internal class EmptyRequestModel
    {
        public static EmptyRequestModel Instance { get; } = new EmptyRequestModel();
    }
}