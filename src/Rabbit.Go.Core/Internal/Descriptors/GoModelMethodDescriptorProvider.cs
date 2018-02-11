using Microsoft.Extensions.Options;
using Rabbit.Go.Core;
using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Core.Internal.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Internal
{
    public class GoModelMethodDescriptorProvider : IMethodDescriptorProvider
    {
        private readonly IList<Type> _types;
        private readonly IReadOnlyList<IGoModelProvider> _modelProviders;
        private readonly IList<IGoModelConvention> _conventions;

        public GoModelMethodDescriptorProvider(
            IEnumerable<IGoModelProvider> modelProviders,
            IOptions<GoOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;
            _types = options.Types.Distinct().ToArray();
            _modelProviders = modelProviders.OrderBy(i => i.Order).ToArray();
            _conventions = options.Conventions;
        }

        #region Implementation of IMethodDescriptorProvider

        public int Order { get; } = 0;

        public void OnProvidersExecuting(MethodDescriptorProviderContext context)
        {
            var model = BuildModel();

            ApplyConventions(model);

            var descriptors = GoModelDescriptorBuilder.Build(model);
            foreach (var descriptor in descriptors)
                context.Results.Add(descriptor);
        }

        public void OnProvidersExecuted(MethodDescriptorProviderContext context)
        {
        }

        #endregion Implementation of IMethodDescriptorProvider

        #region Private Method

        private void ApplyConventions(GoModel model)
        {
            foreach (var convention in _conventions)
                convention.Apply(model);

            foreach (var type in model.Types)
            {
                foreach (var typeModelConvention in type.Attributes.OfType<ITypeModelConvention>())
                    typeModelConvention.Apply(type);

                foreach (var methodModel in type.Methods)
                {
                    foreach (var methodModelConvention in methodModel.Attributes.OfType<IMethodModelConvention>())
                        methodModelConvention.Apply(methodModel);

                    foreach (var parameterModel in methodModel.Parameters)
                        foreach (var parameterModelConvention in parameterModel.Attributes.OfType<IParameterModelConvention>())
                            parameterModelConvention.Apply(parameterModel);
                }
            }
        }

        private GoModel BuildModel()
        {
            var providerContext = new GoModelProviderContext(_types);

            foreach (var provider in _modelProviders)
                provider.OnProvidersExecuting(providerContext);

            for (var i = _modelProviders.Count - 1; i >= 0; i--)
                _modelProviders[i].OnProvidersExecuted(providerContext);

            return providerContext.Result;
        }

        #endregion Private Method
    }
}