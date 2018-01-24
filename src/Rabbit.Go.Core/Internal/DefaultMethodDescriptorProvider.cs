using Microsoft.Extensions.Options;
using Rabbit.Go.Utilities;
using System.Linq;

namespace Rabbit.Go.Internal
{
    public class DefaultMethodDescriptorProvider : IMethodDescriptorProvider
    {
        private readonly GoOptions _options;

        public DefaultMethodDescriptorProvider(IOptions<GoOptions> options)
        {
            _options = options.Value;
        }

        #region Implementation of IMethodDescriptorProvider

        public int Order { get; } = 0;

        public void OnProvidersExecuting(MethodDescriptorProviderContext context)
        {
            foreach (var type in _options.Types.Where(i => i.GetTypeAttribute<GoIgnoreAttribute>() == null))
            {
                foreach (var method in type.GetMethods().Where(i => i.GetTypeAttribute<GoIgnoreAttribute>() == null))
                {
                    context.Results.Add(MethodDescriptorUtilities.Create(type, method));
                }
            }
        }

        public void OnProvidersExecuted(MethodDescriptorProviderContext context)
        {
        }

        #endregion Implementation of IMethodDescriptorProvider
    }
}