namespace Rabbit.Go.Core.GoModels
{
    public interface IGoModelConvention
    {
        void Apply(GoModel model);
    }

    public interface ITypeModelConvention
    {
        void Apply(TypeModel model);
    }

    public interface IMethodModelConvention
    {
        void Apply(MethodModel model);
    }

    public interface IParameterModelConvention
    {
        void Apply(ParameterModel model);
    }
}