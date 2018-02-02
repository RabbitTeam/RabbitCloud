using Microsoft.Extensions.Options;
using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Core.Internal.Descriptors;
using Rabbit.Go.Interceptors;
using System.Collections.Generic;

namespace Rabbit.Go.Core.Internal
{
    public class DefaultGoModelProvider : IGoModelProvider
    {
        private readonly IList<IInterceptorMetadata> _globalInterceptors;

        public DefaultGoModelProvider(IOptions<GoOptions> optionsAccessor)
        {
            _globalInterceptors = optionsAccessor.Value.Interceptors;
        }

        #region Implementation of IGoModelProvider

        public int Order => -1000;

        public void OnProvidersExecuting(GoModelProviderContext context)
        {
            var goModel = context.Result;
            GoModelBuilder.Build(goModel, context.Types);

            foreach (var interceptor in _globalInterceptors)
                goModel.Interceptors.Add(interceptor);
        }

        public void OnProvidersExecuted(GoModelProviderContext context)
        {
        }

        #endregion Implementation of IGoModelProvider
    }
}