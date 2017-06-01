using System;

namespace RabbitCloud.Config.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RefererAttribute : Attribute
    {
        public string Group { get; set; }
        public string Protocol { get; set; }
        public string Registry { get; set; }
        public string Cluster { get; set; }
        public string LoadBalance { get; set; }
        public string HaStrategy { get; set; }
    }
}