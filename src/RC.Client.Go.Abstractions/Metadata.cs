using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions
{
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

        /// <summary>
        /// 如果服务端返回404，则默认返回null而不是抛出异常（这是一个临时的属性）
        /// </summary>
        public bool NotFoundReturnNull { get; set; } = true;
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
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoBodyAttribute : Attribute, IParameterTargetProvider
    {
        #region Implementation of IParameterTargetProvider

        public ParameterTarget Target { get; } = ParameterTarget.Body;

        #endregion Implementation of IParameterTargetProvider
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoParameterAttribute : Attribute, IParameterTargetProvider
    {
        public GoParameterAttribute(ParameterTarget target = ParameterTarget.Query)
        {
            Target = target;
        }

        #region Implementation of IParameterTargetProvider

        public ParameterTarget Target { get; }

        #endregion Implementation of IParameterTargetProvider
    }
}