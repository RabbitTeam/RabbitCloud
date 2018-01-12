using Rabbit.Cloud.Client.Go.Abstractions;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Binder
{
    public class ParameterBindContext
    {
        public GoRequestContext RequestContext { get; set; }
        public ParameterTarget Target { get; set; }
        public Type Type { get; set; }
        public string ModelName { get; set; }
        public object Model { get; set; }
    }

    public interface IParameterBinder
    {
        Task BindAsync(ParameterBindContext context);
    }
}