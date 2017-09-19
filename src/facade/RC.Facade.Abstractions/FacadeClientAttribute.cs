using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FacadeClientAttribute : Attribute
    {
        public FacadeClientAttribute(string nameOrUrl)
        {
            if (Uri.IsWellFormedUriString(nameOrUrl, UriKind.Absolute))
                Url = nameOrUrl;
            else
                Name = nameOrUrl;
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }
}