using Rabbit.Cloud.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Grpc.ApplicationModels.Internal
{
    public static class ApplicationModelConventions
    {
        public static void ApplyConventions(
               ApplicationModel applicationModel,
               IEnumerable<IApplicationModelConvention> conventions)
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
                convention.Apply(applicationModel);

            foreach (var service in applicationModel.Services)
            {
                foreach (var convention in service.Attributes.OfType<IServiceModelConvention>())
                    convention.Apply(service);

                foreach (var method in service.Methods)
                {
                    foreach (var convention in method.Attributes.OfType<IMethodModelConvention>())
                        convention.Apply(method);

                    foreach (var convention in method.Attributes.OfType<ICodecModelConvention>())
                    {
                        convention.Apply(method.RequestCodec);
                        convention.Apply(method.ResponseCodec);
                    }
                    foreach (var convention in method.RequestCodec.Attributes.OfType<ICodecModelConvention>())
                        convention.Apply(method.RequestCodec);
                    foreach (var convention in method.ResponseCodec.Attributes.OfType<ICodecModelConvention>())
                        convention.Apply(method.ResponseCodec);
                }
            }
        }
    }
}