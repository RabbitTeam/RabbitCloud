using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Go.ApplicationModels.Internal
{
    public class ApplicationModelUtilities
    {
        public static ApplicationModel BuildModel(TypeInfo[] serviceTypes, IEnumerable<IApplicationModelProvider> applicationModelProviders)
        {
            var context = new ApplicationModelProviderContext(serviceTypes);
            var providers = applicationModelProviders.OrderBy(i => i.Order).ToArray();

            foreach (var p in providers)
            {
                p.OnProvidersExecuting(context);
            }

            for (var i = providers.Length - 1; i >= 0; i--)
            {
                providers[i].OnProvidersExecuted(context);
            }

            return context.Result;
        }

        public static void ApplyConventions(ApplicationModel applicationModel, IEnumerable<IApplicationModelConvention> conventions)
        {
            if (applicationModel == null)
            {
                throw new ArgumentNullException(nameof(applicationModel));
            }

            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            foreach (var convention in conventions)
            {
                convention.Apply(applicationModel);
            }

            foreach (var service in applicationModel.Services)
            {
                var serviceConventions =
                    service.Attributes
                        .OfType<IServiceModelConvention>()
                        .ToArray();

                foreach (var serviceConvention in serviceConventions)
                {
                    serviceConvention.Apply(service);
                }

                foreach (var request in service.Requests)
                {
                    var requestConventions =
                        request.Attributes
                            .OfType<IRequestModelConvention>()
                            .ToArray();

                    foreach (var requestConvention in requestConventions)
                    {
                        requestConvention.Apply(request);
                    }

                    foreach (var parameter in request.Parameters)
                    {
                        var parameterConventions =
                            parameter.Attributes
                                .OfType<IParameterModelConvention>()
                                .ToArray();

                        foreach (var parameterConvention in parameterConventions)
                        {
                            parameterConvention.Apply(parameter);
                        }
                    }
                }
            }
        }
    }
}