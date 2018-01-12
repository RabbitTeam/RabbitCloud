using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class GoHeaderAttribute : Attribute, IHeadersProvider
    {
        public string Name { get; }
        public string Value { get; }

        public GoHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IHeadersProvider

        public void Collect(IDictionary<string, StringValues> items)
        {
            items[Name] = Value;
        }

        #endregion Implementation of IHeadersProvider
    }

    public interface IItemsProvider
    {
        void Collect(IDictionary<object, object> items);
    }

    public interface IHeadersProvider
    {
        void Collect(IDictionary<string, StringValues> items);
    }

    public interface IPathProvider
    {
        string Path { get; }
    }

    public interface IClientProvider
    {
        string Url { get; }
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
    public class GoRequestAttribute : Attribute, IPathProvider
    {
        public GoRequestAttribute(string path)
        {
            Path = path;
        }

        public GoRequestAttribute()
        {
        }

        #region Implementation of IPathProvider

        public string Path { get; }

        #endregion Implementation of IPathProvider
    }

    public enum ParameterTarget
    {
        Query,
        Header,
        Items,
        Path,
        Body
    }

    public interface IParameterTargetProvider
    {
        ParameterTarget Target { get; }
        string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoBodyAttribute : Attribute, IParameterTargetProvider, IHeadersProvider
    {
        public GoBodyAttribute() : this("application/json")
        {
        }

        public GoBodyAttribute(string contentType)
        {
            ContentType = contentType;
        }

        public string ContentType { get; }

        #region Implementation of IParameterTargetProvider

        public ParameterTarget Target { get; } = ParameterTarget.Body;
        public string Name { get; set; }

        #endregion Implementation of IParameterTargetProvider

        #region Implementation of IHeadersProvider

        public void Collect(IDictionary<string, StringValues> items)
        {
            items["Content-Type"] = ContentType;
        }

        #endregion Implementation of IHeadersProvider
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoParameterAttribute : Attribute, IParameterTargetProvider
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

        #region Implementation of IParameterTargetProvider

        public ParameterTarget Target { get; }
        public string Name { get; }

        #endregion Implementation of IParameterTargetProvider
    }
}