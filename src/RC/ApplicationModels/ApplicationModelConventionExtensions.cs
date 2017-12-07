using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.ApplicationModels
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
            ICodecModelConvention codecModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (codecModelConvention == null)
                throw new ArgumentNullException(nameof(codecModelConvention));

            conventions.Add(new CodecApplicationModelConvention(codecModelConvention));
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
                _methodModelConvention = methodModelConvention ?? throw new ArgumentNullException(nameof(methodModelConvention));
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

        private class CodecApplicationModelConvention : IApplicationModelConvention
        {
            private readonly ICodecModelConvention _codecModelConvention;

            public CodecApplicationModelConvention(ICodecModelConvention codecModelConvention)
            {
                _codecModelConvention = codecModelConvention ?? throw new ArgumentNullException(nameof(codecModelConvention));
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
                        _codecModelConvention.Apply(method.RequestCodec);
                        _codecModelConvention.Apply(method.ResponseCodec);
                    }
                }
            }

            #endregion Implementation of IApplicationModelConvention
        }

        #endregion Help Type
    }
}