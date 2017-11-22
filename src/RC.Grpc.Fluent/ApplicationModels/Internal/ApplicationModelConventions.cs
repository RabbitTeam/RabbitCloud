using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels.Internal
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

                    foreach (var convention in method.Attributes.OfType<IMarshallerModelConvention>())
                    {
                        convention.Apply(method.RequestMarshaller);
                        convention.Apply(method.ResponseMarshaller);
                    }
                    foreach (var convention in method.RequestMarshaller.Attributes.OfType<IMarshallerModelConvention>())
                        convention.Apply(method.RequestMarshaller);
                    foreach (var convention in method.ResponseMarshaller.Attributes.OfType<IMarshallerModelConvention>())
                        convention.Apply(method.ResponseMarshaller);
                }
            }

            foreach (var serverService in applicationModel.ServerServices)
            {
                foreach (var convention in serverService.Attributes.OfType<IServerServiceModelConvention>())
                    convention.Apply(serverService);

                foreach (var serverMethod in serverService.ServerMethods)
                {
                    foreach (var convention in serverMethod.Attributes.OfType<IServerMethodModelConvention>())
                        convention.Apply(serverMethod);

                    foreach (var convention in serverMethod.Attributes.OfType<IMarshallerModelConvention>())
                    {
                        convention.Apply(serverMethod.Method.RequestMarshaller);
                        convention.Apply(serverMethod.Method.ResponseMarshaller);
                    }
                    foreach (var convention in serverMethod.Method.RequestMarshaller.Attributes.OfType<IMarshallerModelConvention>())
                        convention.Apply(serverMethod.Method.RequestMarshaller);
                    foreach (var convention in serverMethod.Method.ResponseMarshaller.Attributes.OfType<IMarshallerModelConvention>())
                        convention.Apply(serverMethod.Method.ResponseMarshaller);
                }
            }
        }
    }
}