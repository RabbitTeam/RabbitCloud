using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Go.Abstractions
{
    public class GoGetAttribute : GoMethodAttribute
    {
        public GoGetAttribute(string path) : base("Get", path)
        {
        }
    }

    public class GoPostAttribute : GoMethodAttribute
    {
        public GoPostAttribute(string path) : base("Post", path)
        {
        }
    }

    public class GoPutAttribute : GoMethodAttribute
    {
        public GoPutAttribute(string path) : base("Put", path)
        {
        }
    }

    public class GoDeleteAttribute : GoMethodAttribute
    {
        public GoDeleteAttribute(string path) : base("Delete", path)
        {
        }
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

        public void Collect(IDictionary<object, object> items)
        {
            items["HttpMethod"] = Method;
        }

        #endregion Implementation of IItemsProvider
    }
}