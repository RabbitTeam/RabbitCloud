using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Models
{
    public class ParameterModel
    {
        public ParameterModel(ParameterInfo parameterInfo, IReadOnlyList<object> attributes)
        {
            ParameterInfo = parameterInfo;
            Attributes = attributes;
            ParameterName = parameterInfo.Name;
        }

        public IReadOnlyList<object> Attributes { get; }
        public ParameterInfo ParameterInfo { get; }

        public string ParameterName { get; set; }
        public RequestModel Request { get; set; }
    }
}