using Rabbit.Go.Core.GoModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModelConventionExtensions
    {
        public static void RemoveType<TGoModelConvention>(this IList<IGoModelConvention> list) where TGoModelConvention : IGoModelConvention
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            RemoveType(list, typeof(TGoModelConvention));
        }

        public static void RemoveType(this IList<IGoModelConvention> list, Type type)
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
                var goModelConvention = list[i];
                if (goModelConvention.GetType() == type)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void Add(
            this IList<IGoModelConvention> conventions,
            ITypeModelConvention typeModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (typeModelConvention == null)
            {
                throw new ArgumentNullException(nameof(typeModelConvention));
            }

            conventions.Add(new TypeModelConvention(typeModelConvention));
        }

        public static void Add(
            this IList<IGoModelConvention> conventions,
            IMethodModelConvention methodModelConvention)
        {
            if (conventions == null)
            {
                throw new ArgumentNullException(nameof(conventions));
            }

            if (methodModelConvention == null)
            {
                throw new ArgumentNullException(nameof(methodModelConvention));
            }

            conventions.Add(new MethodGoModelConvention(methodModelConvention));
        }

        public static void Add(
            this IList<IGoModelConvention> conventions,
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

            conventions.Add(new ParameterGoModelConvention(parameterModelConvention));
        }

        private class ParameterGoModelConvention : IGoModelConvention
        {
            private readonly IParameterModelConvention _parameterModelConvention;

            public ParameterGoModelConvention(IParameterModelConvention parameterModelConvention)
            {
                _parameterModelConvention = parameterModelConvention ?? throw new ArgumentNullException(nameof(parameterModelConvention));
            }

            public void Apply(GoModel model)
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                var types = model.Types.ToArray();
                foreach (var type in types)
                {
                    var methods = type.Methods.ToArray();
                    foreach (var method in methods)
                    {
                        var parameters = method.Parameters.ToArray();
                        foreach (var parameter in parameters)
                        {
                            _parameterModelConvention.Apply(parameter);
                        }
                    }
                }
            }
        }

        private class MethodGoModelConvention : IGoModelConvention
        {
            private readonly IMethodModelConvention _methodModelConvention;

            public MethodGoModelConvention(IMethodModelConvention methodModelConvention)
            {
                _methodModelConvention = methodModelConvention ?? throw new ArgumentNullException(nameof(methodModelConvention));
            }

            public void Apply(GoModel model)
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }
                var types = model.Types.ToArray();
                foreach (var type in types)
                {
                    var methods = type.Methods.ToArray();
                    foreach (var method in methods)
                    {
                        _methodModelConvention.Apply(method);
                    }
                }
            }
        }

        private class TypeModelConvention : IGoModelConvention
        {
            private readonly ITypeModelConvention _typeModelConvention;

            public TypeModelConvention(ITypeModelConvention typeConvention)
            {
                _typeModelConvention = typeConvention ?? throw new ArgumentNullException(nameof(typeConvention));
            }

            public void Apply(GoModel model)
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                var types = model.Types.ToArray();
                foreach (var type in types)
                {
                    _typeModelConvention.Apply(type);
                }
            }
        }
    }
}