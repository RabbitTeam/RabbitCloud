using RabbitCloud.Rpc.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RabbitCloud.Rpc
{
    public class TypeCaller : GroupCaller
    {
        public TypeCaller(object instance)
        {
            Callers = GetMethodCallers(instance).ToArray();
        }

        #region Overrides of GroupCaller

        public override IEnumerable<INamedCaller> Callers { get; }

        #endregion Overrides of GroupCaller

        private IEnumerable<INamedCaller> GetMethodCallers(object instance)
        {
            var callers = new List<INamedCaller>();
            foreach (var methodInfo in instance.GetType().GetRuntimeMethods())
            {
                callers.Add(new MethodCaller(instance, methodInfo, name => callers.All(i => i.Name != name)));
            }
            return callers;
        }
    }
}