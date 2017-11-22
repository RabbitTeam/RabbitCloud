using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public static class ApplicationModelConventionExtensions
    {
        /// <summary>
        /// Removes all application model conventions of the specified type.
        /// </summary>
        /// <param name="list">The list of <see cref="IApplicationModelConvention"/>s.</param>
        /// <typeparam name="TApplicationModelConvention">The type to remove.</typeparam>
        public static void RemoveType<TApplicationModelConvention>(this IList<IApplicationModelConvention> list) where TApplicationModelConvention : IApplicationModelConvention
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            RemoveType(list, typeof(TApplicationModelConvention));
        }

        /// <summary>
        /// Removes all application model conventions of the specified type.
        /// </summary>
        /// <param name="list">The list of <see cref="IApplicationModelConvention"/>s.</param>
        /// <param name="type">The type to remove.</param>
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
                throw new ArgumentNullException(nameof(serviceModelConvention));

            conventions.Add(new ServiceApplicationModelConvention(serviceModelConvention));
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IMethodModelConvention methodModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (methodModelConvention == null)
                throw new ArgumentNullException(nameof(methodModelConvention));

            conventions.Add(new MethodApplicationModelConvention(methodModelConvention));
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IMarshallerModelConvention marshallerModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (marshallerModelConvention == null)
                throw new ArgumentNullException(nameof(marshallerModelConvention));

            conventions.Add(new MarshallerApplicationModelConvention(marshallerModelConvention));
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IServerServiceModelConvention serverServiceModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (serverServiceModelConvention == null)
                throw new ArgumentNullException(nameof(serverServiceModelConvention));

            conventions.Add(new ServerServiceApplicationModelConvention(serverServiceModelConvention));
        }

        public static void Add(
            this IList<IApplicationModelConvention> conventions,
            IServerMethodModelConvention serverMethodModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (serverMethodModelConvention == null)
                throw new ArgumentNullException(nameof(serverMethodModelConvention));

            conventions.Add(new ServerMethodApplicationModelConvention(serverMethodModelConvention));
        }

        #region Help Type

        private class ServiceApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IServiceModelConvention _serviceModelConvention;

            public ServiceApplicationModelConvention(IServiceModelConvention serviceConvention)
            {
                _serviceModelConvention = serviceConvention ?? throw new ArgumentNullException(nameof(serviceConvention));
            }

            #region Implementation of IApplicationModelConvention

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                    throw new ArgumentNullException(nameof(application));

                foreach (var service in application.Services)
                {
                    _serviceModelConvention.Apply(service);
                }
            }

            #endregion Implementation of IApplicationModelConvention
        }

        private class MethodApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IMethodModelConvention _methodModelConvention;

            public MethodApplicationModelConvention(IMethodModelConvention methodModelConvention)
            {
                _methodModelConvention = methodModelConvention ?? throw new ArgumentNullException(nameof(methodModelConvention)); ;
            }

            #region Implementation of IApplicationModelConvention

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                    throw new ArgumentNullException(nameof(application));

                foreach (var service in application.Services)
                {
                    foreach (var method in service.Methods)
                    {
                        _methodModelConvention.Apply(method);
                    }
                }
            }

            #endregion Implementation of IApplicationModelConvention
        }

        private class MarshallerApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IMarshallerModelConvention _marshallerModelConvention;

            public MarshallerApplicationModelConvention(IMarshallerModelConvention marshallerModelConvention)
            {
                _marshallerModelConvention = marshallerModelConvention ?? throw new ArgumentNullException(nameof(marshallerModelConvention)); ;
            }

            #region Implementation of IApplicationModelConvention

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                    throw new ArgumentNullException(nameof(application));

                foreach (var service in application.Services)
                {
                    foreach (var method in service.Methods)
                    {
                        _marshallerModelConvention.Apply(method.RequestMarshaller);
                        _marshallerModelConvention.Apply(method.ResponseMarshaller);
                    }
                }
            }

            #endregion Implementation of IApplicationModelConvention
        }

        private class ServerServiceApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IServerServiceModelConvention _serverServiceModelConvention;

            public ServerServiceApplicationModelConvention(IServerServiceModelConvention serverServiceModelConvention)
            {
                _serverServiceModelConvention = serverServiceModelConvention ?? throw new ArgumentNullException(nameof(serverServiceModelConvention));
            }

            #region Implementation of IApplicationModelConvention

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                    throw new ArgumentNullException(nameof(application));

                foreach (var serverService in application.ServerServices)
                {
                    _serverServiceModelConvention.Apply(serverService);
                }
            }

            #endregion Implementation of IApplicationModelConvention
        }

        private class ServerMethodApplicationModelConvention : IApplicationModelConvention
        {
            private readonly IServerMethodModelConvention _serverMethodModelConvention;

            public ServerMethodApplicationModelConvention(IServerMethodModelConvention serverMethodModelConvention)
            {
                _serverMethodModelConvention = serverMethodModelConvention ?? throw new ArgumentNullException(nameof(serverMethodModelConvention));
            }

            #region Implementation of IApplicationModelConvention

            public void Apply(ApplicationModel application)
            {
                if (application == null)
                    throw new ArgumentNullException(nameof(application));

                foreach (var serverService in application.ServerServices)
                {
                    foreach (var serverMethod in serverService.ServerMethods)
                    {
                        _serverMethodModelConvention.Apply(serverMethod);
                    }
                }
            }

            #endregion Implementation of IApplicationModelConvention
        }

        #endregion Help Type
    }
}