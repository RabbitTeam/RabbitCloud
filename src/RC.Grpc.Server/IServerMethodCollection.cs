using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Server
{
    public class ServiceMethod
    {
        public Delegate Delegate { get; set; }
        public IMethod Method { get; set; }
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
    }

    public interface IServerMethodCollection : IEnumerable<ServiceMethod>
    {
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        int Revision { get; }

        ServiceMethod this[string serviceId] { get; set; }

        ServiceMethod Get(string serviceId);

        void Set(ServiceMethod serviceMethod);
    }
}