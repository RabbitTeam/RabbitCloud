using Grpc.Core;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Client
{
    public interface IMethodCollection : IEnumerable<IMethod>
    {
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        int Revision { get; }

        IMethod this[string serviceId] { get; set; }

        IMethod Get(string serviceId);

        void Set(IMethod method);
    }

    public static class MethodCollectionExtensions
    {
        public static Method<TRequest, TResponse> Get<TRequest, TResponse>(this IMethodCollection collection, string serviceId)
        {
            return collection.Get(serviceId) as Method<TRequest, TResponse>;
        }
    }
}