using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Facade.Models.Internal
{
    internal class ApplicationModelConventions
    {
        public static void ApplyConventions(ApplicationModel applicationModel,
            IEnumerable<IApplicationModelConvention> conventions)
        {
            if (applicationModel == null)
                throw new ArgumentNullException(nameof(applicationModel));

            if (conventions == null)
                throw new ArgumentNullException(nameof(conventions));

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