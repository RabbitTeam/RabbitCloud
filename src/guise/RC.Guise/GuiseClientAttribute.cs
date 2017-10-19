using System;

namespace Rabbit.Cloud.Guise
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class GuiseClientAttribute : Attribute
    {
        public GuiseClientAttribute(string nameOrUrl)
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