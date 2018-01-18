using System;

namespace Rabbit.Go.Abstractions
{
    public class GoGetAttribute : GoMethodAttribute
    {
        public GoGetAttribute() : base("Get")
        {
        }

        public GoGetAttribute(string path) : base("Get", path)
        {
        }
    }

    public class GoPostAttribute : GoMethodAttribute
    {
        public GoPostAttribute() : base("Post")
        {
        }

        public GoPostAttribute(string path) : base("Post", path)
        {
        }
    }

    public class GoPutAttribute : GoMethodAttribute
    {
        public GoPutAttribute() : base("Put")
        {
        }

        public GoPutAttribute(string path) : base("Put", path)
        {
        }
    }

    public class GoDeleteAttribute : GoMethodAttribute
    {
        public GoDeleteAttribute() : base("Delete")
        {
        }

        public GoDeleteAttribute(string path) : base("Delete", path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoMethodAttribute : GoRequestAttribute
    {
        public GoMethodAttribute(string method)
        {
            Method = method;
        }

        public GoMethodAttribute(string method, string path) : base(path)
        {
            Method = method;
        }

        public string Method { get; }
    }
}