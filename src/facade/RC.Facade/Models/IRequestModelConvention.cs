namespace Rabbit.Cloud.Facade.Models
{
    public interface IRequestModelConvention
    {
        void Apply(RequestModel requestModel);
    }
}