using Grpc.Core;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public interface IMethodTable : IEnumerable<IMethod>
    {
        IMethod this[string fullName] { get; set; }

        IMethod Get(string fullName);

        void Set(IMethod method);
    }
}