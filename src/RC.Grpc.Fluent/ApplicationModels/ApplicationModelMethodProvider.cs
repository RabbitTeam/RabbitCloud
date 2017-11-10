using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Fluent.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public class ApplicationModelMethodProviderOptions
    {
        public ICollection<TypeInfo> Types { get; } = new List<TypeInfo>();
    }

    public class ApplicationModelMethodProvider : IMethodProvider
    {
        private readonly ApplicationModelMethodProviderOptions _options;
        private readonly IReadOnlyCollection<IApplicationModelProvider> _applicationModelProviders;

        public ApplicationModelMethodProvider(IEnumerable<IApplicationModelProvider> applicationModelProviders, IOptions<ApplicationModelMethodProviderOptions> options)
        {
            if (applicationModelProviders == null)
                throw new ArgumentNullException(nameof(applicationModelProviders));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _applicationModelProviders = applicationModelProviders.OrderBy(i => i.Order).ToArray();
            _options = options.Value;
        }

        #region Implementation of IMethodProvider

        public int Order { get; } = 10;

        public void OnProvidersExecuting(MethodProviderContext context)
        {
            var applicationModelProviderContext = new ApplicationModelProviderContext(_options.Types);

            foreach (var applicationModelProvider in _applicationModelProviders)
            {
                applicationModelProvider.OnProvidersExecuting(applicationModelProviderContext);
            }

            foreach (var applicationModelProvider in _applicationModelProviders)
            {
                applicationModelProvider.OnProvidersExecuted(applicationModelProviderContext);
            }

            var applicationModel = applicationModelProviderContext.Result;

            foreach (var serviceModel in applicationModel.Services)
            {
                foreach (var methodModel in serviceModel.Methods)
                {
                    var method = methodModel.CreateGenericMethod();
                    context.Results.Add(method);
                }
            }
        }

        public void OnProvidersExecuted(MethodProviderContext context)
        {
        }

        #endregion Implementation of IMethodProvider
    }
}