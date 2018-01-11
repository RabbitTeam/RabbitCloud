using Rabbit.Cloud.Client.Go.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationModelConventionExtensions
    {
        public static void RemoveType<TApplicationModelConvention>(this IList<IApplicationModelConvention> list) where TApplicationModelConvention : IApplicationModelConvention
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            RemoveType(list, typeof(TApplicationModelConvention));
        }

        public static void RemoveType(this IList<IApplicationModelConvention> list, Type type)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                var applicationModelConvention = list[i];
                if (applicationModelConvention.GetType() == type)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IServiceModelConvention serviceModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (serviceModelConvention == null)
            {
                throw new ArgumentNullException(nameof(serviceModelConvention));
            }

            conventions.Add(new ServiceApplicationModelConvention(serviceModelConvention));
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IRequestModelConvention requestModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (requestModelConvention == null)
            {
                throw new ArgumentNullException(nameof(requestModelConvention));
            }

            conventions.Add(new RequestApplicationModelConvention(requestModelConvention));
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IParameterModelConvention parameterModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (parameterModelConvention == null)
            {
                throw new ArgumentNullException(nameof(parameterModelConvention));
            }

            conventions.Add(new ParameterApplicationModelConvention(parameterModelConvention));
        }

        private class ParameterApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IParameterModelConvention _parameterModelConvention;

            public ParameterApplicationModelConvention(IParameterModelConvention parameterModelConvention)
            {
                _parameterModelConvention = parameterModelConvention ?? throw new ArgumentNullException(nameof(parameterModelConvention));
            }

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                {
                    throw new ArgumentNullException(nameof(application));
                }

                var services = application.Services.ToArray();
                foreach (var service in services)
                {
                    var requests = service.Requests.ToArray();
                    foreach (var request in requests)
                    {
                        var parameters = request.Parameters.ToArray();
                        foreach (var parameter in parameters)
                        {
                            _parameterModelConvention.Apply(parameter);
                        }
                    }
                }
            }
        }

        private class RequestApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IRequestModelConvention _requestModelConvention;

            public RequestApplicationModelConvention(IRequestModelConvention requestModelConvention)
            {
                _requestModelConvention = requestModelConvention ?? throw new ArgumentNullException(nameof(requestModelConvention));
            }

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                {
                    throw new ArgumentNullException(nameof(application));
                }

                var services = application.Services.ToArray();
                foreach (var service in services)
                {
                    var requests = service.Requests.ToArray();
                    foreach (var request in requests)
                    {
                        _requestModelConvention.Apply(request);
                    }
                }
            }
        }

        private class ServiceApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IServiceModelConvention _serviceModelConvention;

            public ServiceApplicationModelConvention(IServiceModelConvention serviceModelConvention)
            {
                _serviceModelConvention = serviceModelConvention ?? throw new ArgumentNullException(nameof(serviceModelConvention));
            }

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                {
                    throw new ArgumentNullException(nameof(application));
                }

                var services = application.Services.ToArray();
                foreach (var service in services)
                {
                    _serviceModelConvention.Apply(service);
                }
            }
        }
    }
}