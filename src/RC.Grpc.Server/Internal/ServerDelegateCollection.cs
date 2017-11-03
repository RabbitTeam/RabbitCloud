using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Server.Internal
{
    public class ServerDelegateCollection : IServerMethodCollection
    {
        private IDictionary<string, ServiceMethod> _serviceMethods;
        private volatile int _containerRevision;

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ServiceMethod> GetEnumerator()
        {
            return _serviceMethods.Values.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Implementation of IServerDelegateCollection

        /// <inheritdoc />
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        public int Revision => _containerRevision;

        public ServiceMethod this[string serviceId]
        {
            get
            {
                if (serviceId == null)
                {
                    throw new ArgumentNullException(nameof(serviceId));
                }

                return _serviceMethods != null && _serviceMethods.TryGetValue(serviceId, out var result) ? result : null;
            }
            set
            {
                if (serviceId == null)
                {
                    throw new ArgumentNullException(nameof(serviceId));
                }

                if (value == null)
                {
                    if (_serviceMethods != null && _serviceMethods.Remove(serviceId))
                    {
                        _containerRevision++;
                    }
                    return;
                }

                if (_serviceMethods == null)
                {
                    _serviceMethods = new Dictionary<string, ServiceMethod>();
                }
                _serviceMethods[serviceId] = value;
                _containerRevision++;
            }
        }

        public ServiceMethod Get(string serviceId)
        {
            return this[serviceId];
        }

        public void Set(ServiceMethod serviceMethod)
        {
            this[serviceMethod.Method.FullName] = serviceMethod;
        }

        #endregion Implementation of IServerDelegateCollection
    }
}