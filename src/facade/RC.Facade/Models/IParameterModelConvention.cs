namespace Rabbit.Cloud.Facade.Models
{
    public interface IParameterModelConvention
    {
        void Apply(ParameterModel parameter);
    }
}