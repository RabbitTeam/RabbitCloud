using Rabbit.Go.Abstractions;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go.Binder
{
    public class ParameterBindContext
    {
        public RequestContext RequestContext { get; set; }
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