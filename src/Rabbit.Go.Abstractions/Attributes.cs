using System;

namespace Rabbit.Go
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class GoAttribute : Attribute
    {
        public GoAttribute(string url)
        {
            Url = url;
        }

        public string Url { get; set; }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class GoIgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoParameterAttribute : Attribute
    {
        public string Name { get; set; }
        public ParameterTarget Target { get; set; }

        public GoParameterAttribute(ParameterTarget target) : this(null, target)
        {
        }

        public GoParameterAttribute(string name, ParameterTarget target)
        {
            Name = name;
            Target = target;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoRequestAttribute : Attribute
    {
        public GoRequestAttribute(string path) : this(path, "GET")
        {
        }

        public GoRequestAttribute(string path, string method)
        {
            Path = path;
            Method = method;
        }

        public string Path { get; set; }
        public string Method { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoGetAttribute : GoRequestAttribute
    {
        public GoGetAttribute(string path) : base(path, "GET")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoPostAttribute : GoRequestAttribute
    {
        public GoPostAttribute() : this(string.Empty)
        {
        }

        public GoPostAttribute(string path) : base(path, "POST")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoPutAttribute : GoRequestAttribute
    {
        public GoPutAttribute(string path) : base(path, "PUT")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoDeleteAttribute : GoRequestAttribute
    {
        public GoDeleteAttribute(string path) : base(path, "DELETE")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoPatchAttribute : GoRequestAttribute
    {
        public GoPatchAttribute(string path) : base(path, "PATCH")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoBodyAttribute : GoParameterAttribute
    {
        public GoBodyAttribute() : base(ParameterTarget.Body)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoQueryAttribute : GoParameterAttribute
    {
        public GoQueryAttribute() : this(null)
        {
        }

        public GoQueryAttribute(string name) : base(name, ParameterTarget.Query)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoPathAttribute : GoParameterAttribute
    {
        public GoPathAttribute() : this(null)
        {
        }

        public GoPathAttribute(string name) : base(name, ParameterTarget.Path)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoHeaderAttribute : GoParameterAttribute
    {
        public GoHeaderAttribute() : this(null)
        {
        }

        public GoHeaderAttribute(string name) : base(name, ParameterTarget.Header)
        {
        }
    }
}