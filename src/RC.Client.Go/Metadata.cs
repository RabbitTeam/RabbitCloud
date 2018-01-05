using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Rabbit.Cloud.Client.Go
{
    public class GoGetAttribute : GoMethodAttribute
    {
        public GoGetAttribute(string path) : base(HttpMethod.Get.Method, path)
        {
        }
    }

    public class GoPostAttribute : GoMethodAttribute
    {
        public GoPostAttribute(string path) : base(HttpMethod.Post.Method, path)
        {
        }
    }

    public class GoPutAttribute : GoMethodAttribute
    {
        public GoPutAttribute(string path) : base(HttpMethod.Put.Method, path)
        {
        }
    }

    public class GoDeleteAttribute : GoMethodAttribute
    {
        public GoDeleteAttribute(string path) : base(HttpMethod.Delete.Method, path)
        {
        }
    }

    public class ItemProviderContext
    {
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();
    }

    public interface IItemsProvider
    {
        void Collect(ItemProviderContext context);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GoMethodAttribute : GoRequestAttribute, IItemsProvider
    {
        public GoMethodAttribute(string method)
        {
            Method = method;
        }

        public GoMethodAttribute(string method, string path) : base(path)
        {
            Method = method;
        }

        public string Method { get; set; }

        #region Implementation of IItemsProvider

        public void Collect(ItemProviderContext context)
        {
            context.Items["HttpMethod"] = Method;
        }

        #endregion Implementation of IItemsProvider
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class GoClientAttribute : Attribute
    {
        public GoClientAttribute()
        {
        }

        public GoClientAttribute(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }

    public interface IPathProvider
    {
        string Path { get; }
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

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoBodyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class GoParameterAttribute : Attribute
    {
    }
}