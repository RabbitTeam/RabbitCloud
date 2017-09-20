using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Abstractions;
using Rabbit.Cloud.Facade.Models;
using Rabbit.Cloud.Facade.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class ApplicationServiceDescriptorProvider : IServiceDescriptorProvider
    {
        private readonly IApplicationModelProvider[] _applicationModelProviders;
        private readonly IEnumerable<IApplicationModelConvention> _conventions;

        public ApplicationServiceDescriptorProvider(IEnumerable<IApplicationModelProvider> applicationModelProviders, IOptions<FacadeOptions> optionsAccessor)
        {
            if (applicationModelProviders == null)
                throw new ArgumentNullException(nameof(applicationModelProviders));

            _applicationModelProviders = applicationModelProviders.OrderBy(i => i.Order).ToArray();
            _conventions = optionsAccessor.Value.Conventions;
        }

        #region Implementation of IServiceDescriptorProvider

        public int Order { get; } = -1000;

        public void OnProvidersExecuting(ServiceDescriptorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var descriptor in GetDescriptors())
            {
                context.Results.Add(descriptor);
            }
        }

        public void OnProvidersExecuted(ServiceDescriptorProviderContext context)
        {
        }

        #endregion Implementation of IServiceDescriptorProvider

        #region Private Method

        private IEnumerable<ServiceDescriptor> GetDescriptors()
        {
            var application = BuildModel();
            ApplicationModelConventions.ApplyConventions(application, _conventions);
            return ServiceDescriptorBuilder.Build(application);
        }

        private ApplicationModel BuildModel()
        {
            var facadeTypes = GetFacadeTypes();
            var context = new ApplicationModelProviderContext(facadeTypes);

            foreach (var provider in _applicationModelProviders)
            {
                provider.OnProvidersExecuting(context);
            }

            for (var i = _applicationModelProviders.Length - 1; i >= 0; i--)
            {
                _applicationModelProviders[i].OnProvidersExecuted(context);
            }

            return context.Result;
        }

        public static IEnumerable<TypeInfo> GetFacadeTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(i => !i.IsDynamic).SelectMany(i => i.ExportedTypes).Where(i =>
                  i.IsInterface && i.IsPublic && i.GetCustomAttribute<FacadeClientAttribute>() != null).Select(i => i.GetTypeInfo());
        }

        #endregion Private Method
    }
}