using Grpc.Core;
using Rabbit.Cloud.Grpc.Abstractions.Server;
using System.Collections;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Server.Internal
{
    public class DefaultServerServiceDefinitionTable : IServerServiceDefinitionTable
    {
        private IList<ServerServiceDefinition> _items;

        #region Implementation of IServerServiceDefinitionTable

        public void Set(ServerServiceDefinition definition)
        {
            if (definition == null)
                return;
            if (_items == null)
                _items = new List<ServerServiceDefinition>();

            if (!_items.Contains(definition))
                _items.Add(definition);
        }

        #endregion Implementation of IServerServiceDefinitionTable

        #region Implementation of IEnumerable

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ServerServiceDefinition> GetEnumerator()
        {
            return _items?.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Implementation of IEnumerable
    }
}