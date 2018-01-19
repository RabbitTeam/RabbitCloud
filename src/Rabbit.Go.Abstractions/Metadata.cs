using System;

namespace Rabbit.Go.Abstractions
{
    public interface IClientProvider
    {
        string Url { get; }
    }

    public interface IRequestProvider
    {
        string Path { get; }
    }

    public enum ParameterTarget
    {
        Query,
        Header,
        Items,
        Path,
        Body
    }

    public interface IParameterProvider
    {
        ParameterTarget Target { get; }
        string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class GoClientAttribute : Attribute, IClientProvider
    {
        public GoClientAttribute()
        {
        }

        public GoClientAttribute(string url)
        {
            Url = url;
        }

        #region Implementation of IClientProvider

        public string Url { get; }

        #endregion Implementation of IClientProvider
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoRequestAttribute : Attribute, IRequestProvider
    {
        public GoRequestAttribute(string path)
        {
            Path = path;
        }

        public GoRequestAttribute()
        {
        }

        #region Implementation of IRequestProvider

        public string Path { get; }

        #endregion Implementation of IRequestProvider
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoBodyAttribute : Attribute, IParameterProvider
    {
        #region Implementation of IParameterProvider

        public ParameterTarget Target { get; } = ParameterTarget.Body;
        public string Name { get; set; }

        #endregion Implementation of IParameterProvider
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoParameterAttribute : Attribute, IParameterProvider
    {
        public GoParameterAttribute()
        {
            Target = ParameterTarget.Query;
        }

        public GoParameterAttribute(string name) : this(ParameterTarget.Query, name)
        {
        }

        public GoParameterAttribute(ParameterTarget target) : this(target, null)
        {
        }

        public GoParameterAttribute(ParameterTarget target, string name)
        {
            Target = target;
            Name = name;
        }

        #region Implementation of IParameterProvider

        public ParameterTarget Target { get; set; }
        public string Name { get; set; }

        #endregion Implementation of IParameterProvider
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoQueryAttribute : GoParameterAttribute
    {
        public GoQueryAttribute() : base(ParameterTarget.Query)
        {
        }

        public GoQueryAttribute(string name) : this()
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoHeaderAttribute : GoParameterAttribute
    {
        public GoHeaderAttribute() : base(ParameterTarget.Header)
        {
        }

        public GoHeaderAttribute(string name) : this()
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoPathAttribute : GoParameterAttribute
    {
        public GoPathAttribute() : base(ParameterTarget.Path)
        {
        }

        public GoPathAttribute(string name) : this()
        {
            Name = name;
        }
    }
}