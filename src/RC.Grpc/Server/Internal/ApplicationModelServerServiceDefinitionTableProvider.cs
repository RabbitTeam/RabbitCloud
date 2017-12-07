using Rabbit.Cloud.Grpc.Abstractions.Server;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Grpc.Server.Internal
{
    public class ApplicationModelServerServiceDefinitionTableProvider : IServerServiceDefinitionTableProvider
    {
        private readonly IReadOnlyCollection<IServerServiceDefinitionProvider> _providers;
        private IServerServiceDefinitionTable _table;

        public ApplicationModelServerServiceDefinitionTableProvider(IEnumerable<IServerServiceDefinitionProvider> providers)
        {
            _providers = providers.OrderBy(i => i.Order).ToArray();
        }

        #region Implementation of IServerServiceDefinitionTableProvider

        public IServerServiceDefinitionTable ServerServiceDefinitionTable
        {
            get
            {
                if (_table != null)
                    return _table;
                return _table = CreateTable();
            }
        }

        #endregion Implementation of IServerServiceDefinitionTableProvider

        private IServerServiceDefinitionTable CreateTable()
        {
            var context = new ServerServiceDefinitionProviderContext();
            foreach (var provider in _providers)
            {
                provider.OnProvidersExecuting(context);
            }
            foreach (var provider in _providers)
            {
                provider.OnProvidersExecuted(context);
            }

            var table = new DefaultServerServiceDefinitionTable();
            foreach (var definition in context.Results)
            {
                table.Set(definition);
            }

            return table;
        }
    }
}