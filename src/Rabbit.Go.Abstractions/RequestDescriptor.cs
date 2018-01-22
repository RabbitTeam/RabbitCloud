using System;

namespace Rabbit.Go.Abstractions
{
    public class RequestDescriptor
    {
        public Type ReturnType { get; set; }
        public Type BodyType { get; set; }
    }
}