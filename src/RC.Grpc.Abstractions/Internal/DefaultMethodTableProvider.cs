using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Grpc.Abstractions.Internal
{
    public class DefaultMethodTableProvider : IMethodTableProvider
    {
        private readonly IReadOnlyCollection<IMethodProvider> _methodProviders;
        private IMethodTable _methodTable;

        public DefaultMethodTableProvider(IEnumerable<IMethodProvider> methodProviders)
        {
            _methodProviders = methodProviders.OrderBy(i => i.Order).ToArray();
        }

        #region Implementation of IMethodTableProvider

        public IMethodTable MethodTable
        {
            get
            {
                if (_methodTable != null)
                    return _methodTable;
                return _methodTable = CreateMethodTable();
            }
        }

        #endregion Implementation of IMethodTableProvider

        private IMethodTable CreateMethodTable()
        {
            var methodProviderContext = new MethodProviderContext();
            foreach (var methodProvider in _methodProviders)
            {
                methodProvider.OnProvidersExecuting(methodProviderContext);
            }
            foreach (var methodProvider in _methodProviders)
            {
                methodProvider.OnProvidersExecuted(methodProviderContext);
            }

            var methodTable = new DefaultMethodTable();
            foreach (var result in methodProviderContext.Results)
            {
                methodTable.Set(result);
            }

            return methodTable;
        }
    }
}