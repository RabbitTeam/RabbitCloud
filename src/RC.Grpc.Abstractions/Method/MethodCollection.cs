using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Abstractions.Method
{
    public class MethodCollection : IMethodCollection
    {
        private IDictionary<string, IMethod> _methods;
        private volatile int _containerRevision;

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IMethod> GetEnumerator()
        {
            return _methods.Values.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable

        #region Implementation of IMethodCollection

        /// <inheritdoc />
        /// <summary>
        /// Incremented for each modification and can be used to verify cached results.
        /// </summary>
        public int Revision => _containerRevision;

        public IMethod this[string serviceId]
        {
            get
            {
                if (serviceId == null)
                {
                    throw new ArgumentNullException(nameof(serviceId));
                }

                return _methods != null && _methods.TryGetValue(serviceId, out var result) ? result : null;
            }
            set
            {
                if (serviceId == null)
                {
                    throw new ArgumentNullException(nameof(serviceId));
                }

                if (value == null)
                {
                    if (_methods != null && _methods.Remove(serviceId))
                    {
                        _containerRevision++;
                    }
                    return;
                }

                if (_methods == null)
                {
                    _methods = new Dictionary<string, IMethod>();
                }
                _methods[serviceId] = value;
                _containerRevision++;
            }
        }

        public IMethod Get(string serviceId)
        {
            return this[serviceId];
        }

        public void Set(IMethod method)
        {
            this[method.FullName] = method;
        }

        #endregion Implementation of IMethodCollection
    }
}